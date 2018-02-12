using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LpSolveNativeInterface;
using SolverFoundation.Plugin.LpSolve;
using DfsBase;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        /// <summary>
        /// Generate an optimal lineup which maximizes for projected points
        /// </summary>
        /// <param name="playerPool">all players, including already selected ones</param>
        /// <param name="lineupSlots">all lineup positions with the count of how many to draft</param>
        /// <param name="salaryCap">salary cap for the contest</param>
        /// <returns></returns>
        public static LineupSolution Solve(IList<Player> playerPool, IList<LineupSlot> lineupSlots, int salaryCap)
        {
            // deal with pre filled slots
            var prefilled = playerPool.Where(p => p.Chosen).ToList();
            var availablePlayers = playerPool.Except(prefilled).ToList();
            var unfilledCap = salaryCap - prefilled.Sum(p => p.Salary);
            var unfilledSlots = new List<LineupSlot> ();
            foreach (var slot in lineupSlots)
            {
                var prefilledCount = prefilled.Count(p => p.ChosenPosition == slot.Name);
                var newCountForSlot = slot.Count - prefilledCount;
                unfilledSlots.Add(new LineupSlot
                {
                    Name = slot.Name, Count = newCountForSlot
                });
            }

            // trim the draft pool by removing those whose draft positions don't need to be filled
            var unfilledPosNames = unfilledSlots.Where(ls => ls.Count > 0).Select(ls => ls.Name).ToList();            
            availablePlayers = availablePlayers.Where(
                    p => unfilledPosNames.Intersect(p.Positions).Any()).ToList();

            var positionHelpers = new List<PositionHelper>();
            var context = SolverContext.GetContext();
            ConfigureLpSolve(context);

            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters and Decisions----
            var salary = CreateAndBindParameter(availablePlayers, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(availablePlayers, model, players, "ProjectedPointsAsInt");

            // For each lineup position, define 1 param and 1 decision
            // the parameter value indicates whether or not the player is eligible to play at that lineup position
            for (var i = 0; i < unfilledSlots.Count; i++)
            {
                var decisionProp = $"ChosenAtPosition{i}";
                var paramProp = $"EligibleAtPosition{i}";
                positionHelpers.Add(new PositionHelper
                {
                    Decision = CreateAndBindDecision(availablePlayers, model, players, decisionProp),
                    Parameter = CreateAndBindParameter(availablePlayers, model, players, paramProp),
                    Name = unfilledSlots[i].Name,
                    Count = unfilledSlots[i].Count
                });
            }
            var decisions = positionHelpers.Select(ph => ph.Decision).ToList();

            // ---- Define Constraints ----
            // Reserve the right number of each position.
            foreach (var ph in positionHelpers)
            {
                model.AddConstraint($"constraint_{ph.Name}", Model.Sum(
                        Model.ForEach(players, i => ph.Decision[i] * ph.Parameter[i])
                    ) == ph.Count);
            }

            // players can only be in the player pool at 1 position
            model.AddConstraint("maxOf1PositionPerPlayer",
                Model.ForEach(players, i => NumPositionsForPlayer(i, decisions) <= 1));

            // within the salary cap
            var sumOfSalaries = Model.Sum(
                Model.ForEach(players, i => NumPositionsForPlayer(i, decisions) * salary[i])
            );
            model.AddConstraint("withinSalaryCap", sumOfSalaries <= unfilledCap);

            // ---- Define the Goal ----
            // maximize for projectedPoints
            var sumOfProjectedPoints = Model.Sum(
                Model.ForEach(players, i => NumPositionsForPlayer(i, decisions) * projectedPoints[i])
            );
            model.AddGoal("maxPoints", GoalKind.Maximize, sumOfProjectedPoints);

            // Find that lineup
            var simplex = new LpSolveDirective
            {
                TimeLimit = 10000, // timeout after 10 seconds
                LpSolveVerbose = 0, // 4
                LpSolveMsgFunc = LpSolveMsgFunc,
                LpSolveLogFunc = LpSolveLogFunc
            };
            var solution = context.Solve(simplex);
            Log(solution.GetReport().ToString());
            if (solution.Quality == SolverQuality.Infeasible || solution.Quality == SolverQuality.InfeasibleOrUnbounded ||
                solution.Quality == SolverQuality.Unbounded || solution.Quality == SolverQuality.Unknown)
            {
                Log("No Solution! Timeout or infeasible");
                return null;
            }
            context.PropagateDecisions();
            ReportSolution(playerPool, lineupSlots, prefilled, availablePlayers);
            var lineup = playerPool.Where(p => p.Chosen).ToList();
            if (lineup.Count == lineupSlots.Sum(ls => ls.Count))
                return new LineupSolution
                {
                    Lineup = lineup,
                    IsOptimal = solution.Quality == SolverQuality.Optimal
                };

            Log("No Solution! Count of players doesn't match.");
            return null;
        }

        private static void ConfigureLpSolve(SolverContext context)
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var solvers = context.RegisteredSolvers;
            var milpSolver = new SolverRegistration("LpSolveMIP", SolverCapability.MILP,
                "Microsoft.SolverFoundation.Services.ILinearSolver", $"{executingDirectory}\\LpSolvePlugin.dll",
                "SolverFoundation.Plugin.LpSolve.LpSolveSolver", "SolverFoundation.Plugin.LpSolve.LpSolveDirective",
                "SolverFoundation.Plugin.LpSolve.LpSolveParams");
            solvers.Add(milpSolver);
        }

        // sums up the number of positions that a player is chosen into
        // in the final solution this will always be 0 or 1, but in intermediate
        // decisions it could be 2 or more
        private static Term NumPositionsForPlayer(Term i, IList<Decision> decisions)
        {
            var decisionCount = decisions.Count;
            var sum = new SumTermBuilder(decisionCount);
            var sumOfPositions = new SumTermBuilder(decisionCount);
            for (var j = 0; j < decisionCount; j++)
            {
                sumOfPositions.Add(decisions[j][i]);
            }
            sum.Add(sumOfPositions.ToTerm());
            return sum.ToTerm();
        }

        private static void LpSolveLogFunc(IntPtr lp, int userhandle, string buffer)
        {
            Log(buffer);
        }

        private static void LpSolveMsgFunc(IntPtr lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            Log("Msg: " + message);
        }

        private static Decision CreateAndBindDecision(IEnumerable<Player> playerData, Model model, Set players, string bindingProperty)
        {
            // decisions in this particular model are always 0 (false) or 1 (true)
            var chooseP = new Decision(Domain.IntegerRange(0, 1), bindingProperty, players);
            chooseP.SetBinding(playerData, bindingProperty, "Id");
            model.AddDecision(chooseP);
            return chooseP;
        }

        private static Parameter CreateAndBindParameter(IEnumerable<Player> playerData, Model model, Set players, string bindingProperty)
        {
            var param = new Parameter(Domain.Integer, bindingProperty, players);
            param.SetBinding(playerData, bindingProperty, "Id");
            model.AddParameter(param);
            return param;
        }

        private static void ReportSolution(ICollection<Player> playerPool,
            IList<LineupSlot> lineupSlots, 
            ICollection<Player> prefilled, ICollection<Player> available)
        {
            Log("====================================================");
            Log($"Pre-filled players: {prefilled.Count}");
            Log("=========================================");
            foreach (var lineupSlot in lineupSlots)
            {
                var atSlot = prefilled.Where(s => s.ChosenPosition == lineupSlot.Name);
                foreach (var s in atSlot)
                {
                    Log(s.ToString());
                }
            }
            Log("=========================================");

            var selected = playerPool.Where(p => p.Chosen).ToList();
            Log($"Player Pool Size: {playerPool.Count}, After prefill exclusions: {available.Count}");
            Log("====================================================");
            Log("Lineup");
            Log("====================================================");
            var totalProjectedPoints = 0m;
            var totalSalary = 0;
            foreach (var lineupSlot in lineupSlots)
            {
                var atSlot = selected.Where(s => s.ChosenPosition == lineupSlot.Name);
                foreach (var s in atSlot)
                {
                    totalProjectedPoints += s.ProjectedPoints;
                    totalSalary += s.Salary;
                    Log(s.ToString());
                }
            }
            Log("=========================================");
            Log($"Projected Points: {totalProjectedPoints}, Used Salary: {totalSalary}");
            Log("====================================================");
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
