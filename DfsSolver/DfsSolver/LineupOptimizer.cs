using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LpSolveNativeInterface;
using SolverFoundation.Plugin.LpSolve;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static Solution Solve(IList<Player> playerPool, IList<LineupSlot> lineupSlots, int salaryCap)
        {
            var positionHelpers = new List<PositionHelper>();
            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters and Decisions----
            var salary = CreateAndBindParameter(playerPool, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(playerPool, model, players, "ProjectedPoints");

            // For each lineup position, define 1 param and 1 decision
            // the parameter value indicates whether or not the player is eligible to play at that lineup position
            for (var i = 0; i < lineupSlots.Count; i++)
            {
                var decisionProp = $"ChosenAtPosition{i}";
                var paramProp = $"EligibleAtPosition{i}";
                positionHelpers.Add(new PositionHelper
                {
                    Decision = CreateAndBindDecision(playerPool, model, players, decisionProp),
                    Parameter = CreateAndBindParameter(playerPool, model, players, paramProp),
                    Name = lineupSlots[i].Name,
                    Count = lineupSlots[i].Count
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
            model.AddConstraint("withinSalaryCap", sumOfSalaries <= salaryCap);

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
                //LpSolveMsgFunc = LpSolveMsgFunc,
                //LpSolveVerbose = 4,
                //LpSolveLogFunc = LpSolveLogFunc
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
            var lineup = GetSolutionLineup(playerPool, lineupSlots);
            if (lineup.Count == lineupSlots.Sum(ls => ls.Count))
                return new Solution
                {
                    Lineup = lineup,
                    IsOptimal = solution.Quality == SolverQuality.Optimal
                };

            Log("No Solution! Count of players doesn't match.");
            return null;
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

        //private static void LpSolveLogFunc(int lp, int userhandle, string buffer)
        //{
        //    Log(buffer);
        //}

        //private static void LpSolveMsgFunc(int lp, int userhandle, lpsolve.lpsolve_msgmask message)
        //{
        //    Log("Msg: " + message);
        //}

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

        private static IList<Player> GetSolutionLineup(ICollection<Player> playerPool, IList<LineupSlot> lineupSlots)
        {
            var selected = playerPool.Where(p => p.Chosen).ToList();

            Log($"Player Pool: {playerPool.Count} total:");
            var totalProjectedPoints = 0;
            var totalSalary = 0;
            foreach (var lineupSlot in lineupSlots)
            {
                var selectedAtSlot = selected.Where(s => s.ChosenPosition == lineupSlot.Name);
                foreach (var s in selectedAtSlot)
                {
                    totalProjectedPoints += s.ProjectedPoints;
                    totalSalary += s.Salary;
                    Log(s.ToString());
                }
            }
            Log($"Projected Points: {totalProjectedPoints}, Used Salary: {totalSalary}");
   
            return selected;
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
