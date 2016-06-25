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
        public static Solution Solve(IList<Player> playerPool, Dictionary<string, int> rosterSlots, int salaryCap)
        {
            var slotList = rosterSlots.ToList();
            var positionHelpers = new List<PositionHelper>();
            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            var salary = CreateAndBindParameter(playerPool, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(playerPool, model, players, "ProjectedPoints");

            // Positions
            var pitcher = CreateAndBindParameter(playerPool, model, players, "IsPitcherVal");
            var chooseP = CreateAndBindDecision(playerPool, model, players, "ChosenPitcher");
            positionHelpers.Add(new PositionHelper
            {
                Decision = chooseP,
                Parameter = pitcher,
                Name = "P",
                Count = rosterSlots["P"]
            });

            var chooseC = CreateAndBindDecision(playerPool, model, players, "ChosenCatcher");
            var catcher = CreateAndBindParameter(playerPool, model, players, "IsCatcherVal");
            positionHelpers.Add(new PositionHelper
            {
                Decision = chooseC,
                Parameter = catcher,
                Name = "C",
                Count = rosterSlots["C"]
            });

            var firstBaseman = CreateAndBindParameter(playerPool, model, players, "Is1BVal");
            var choose1B = CreateAndBindDecision(playerPool, model, players, "Chosen1B");
            positionHelpers.Add(new PositionHelper
            {
                Decision = choose1B,
                Parameter = firstBaseman,
                Name = "1B",
                Count = rosterSlots["1B"]
            });
            var decisions = positionHelpers.Select(ph => ph.Decision).ToList();

            //var secondBaseman = CreateAndBindParameter(playerPool, model, players, "Is2BVal");
            //var thirdBaseman = CreateAndBindParameter(playerPool, model, players, "Is3BVal");
            //var shortstop = CreateAndBindParameter(playerPool, model, players, "IsSSVal");
            //var leftFielders = CreateAndBindParameter(playerPool, model, players, "IsLFVal");
            //var centerFielders = CreateAndBindParameter(playerPool, model, players, "IsCFVal");
            //var rightFielders = CreateAndBindParameter(playerPool, model, players, "IsRFVal");

            // ---- Define Decisions ----
            // For each position, if player is choosen for that position, 1 if true, and 0 if false
            //model.AddConstraint("catchers", Model.Sum(Model.ForEach(players, i => chooseC[i] * catcher[i])) == slotList[1].Value);
            //var choose1B = CreateAndBindDecision(playerPool, model, players, "Chosen1B");
            //var choose2B = CreateAndBindDecision(playerPool, model, players, "Chosen2B");
            //var choose3B = CreateAndBindDecision(playerPool, model, players, "Chosen3B");
            //var chooseSS = CreateAndBindDecision(playerPool, model, players, "ChosenSS");
            //var chooseLF = CreateAndBindDecision(playerPool, model, players, "ChosenLF");
            //var chooseCF = CreateAndBindDecision(playerPool, model, players, "ChosenCF");
            //var chooseRF = CreateAndBindDecision(playerPool, model, players, "ChosenRF");

            // ---- Define Constraints ----
            // Reserve the right number of each position.
            foreach (var ph in positionHelpers)
            {
                model.AddConstraint($"constraint_{ph.Name}", Model.Sum(
                        Model.ForEach(players, i => ph.Decision[i] * ph.Parameter[i])
                    ) == ph.Count);
            }
            //model.AddConstraint("pitchers", Model.Sum(Model.ForEach(players, i => chooseP[i] * pitcher[i])) == slotList[0].Value);
            ////var cc = slotList.Count > 1 ? slotList[1].Value : 0;
            //model.AddConstraint("catchers", Model.Sum(Model.ForEach(players, i => chooseC[i] * catcher[i])) == slotList[1].Value);
            //model.AddConstraint("first_basemen", Model.Sum(Model.ForEach(players, i => choose1B[i] * firstBaseman[i])) == rosterSlots["1B"]);
            //model.AddConstraint("second_basemen", Model.Sum(Model.ForEach(players, i => choose2B[i] * secondBaseman[i])) == rosterSlots["2B"]);
            //model.AddConstraint("third_basemen", Model.Sum(Model.ForEach(players, i => choose3B[i] * thirdBaseman[i])) == rosterSlots["3B"]);
            //model.AddConstraint("shortstop", Model.Sum(Model.ForEach(players, i => chooseSS[i] * shortstop[i])) == rosterSlots["SS"]);
            //model.AddConstraint("leftFielders", Model.Sum(Model.ForEach(players, i => chooseLF[i] * leftFielders[i])) == rosterSlots["LF"]);
            //model.AddConstraint("centerFielders", Model.Sum(Model.ForEach(players, i => chooseCF[i] * centerFielders[i])) == rosterSlots["CF"]);
            //model.AddConstraint("rightFielders", Model.Sum(Model.ForEach(players, i => chooseRF[i] * rightFielders[i])) == rosterSlots["RF"]);

            // total size of lineup
            //model.AddConstraints("lineupSize", AllPlayers(decisions, playerPool) == rosterSlots.Sum(rs => rs.Value));

            // within the salary cap
            //model.AddConstraint("maxSalary", SumOfSalaries(decisions, playerPool, players) <= salaryCap);
            //model.AddConstraint("maxSalary", Model.Sum(Model.ForEach(players, i => NumPositions(i, decisions) * salary[i])) <= salaryCap);
            //var sumOfSalaries = Model.Sum(Model.ForEach(players, i =>
            //    (chooseP[i] + chooseC[i] + choose1B[i]) *
            //    salary[i])
            //);
            //// within the salary cap
            //model.AddConstraint("withinSalaryCap", Model.Sum(Model.ForEach(players, i => Model.If(chooseP[i] > 0, salary[i], 0))) <= unfilledCap);
            // within the salary cap
            var sumOfSalaries = Model.Sum(Model.ForEach(players, i =>
                (chooseP[i] + chooseC[i] + choose1B[i]) * salary[i])
            );
            model.AddConstraint("maxSalary", sumOfSalaries <= salaryCap);
            //model.AddConstraint("maxSalary", sumOfSalaries <= salaryCap);
            // same player can't be in 2 roster spots at the same time
            //foreach (var player in playerPool)
            //{
            //    model.AddConstraint($"unique_{player.Id}", Model.ForEach(players, i => UniquePosition(i, decisions)));
            //}

            // ---- Define the Goal ----
            // maximize for projectedPoints
            //var sumOfProjectedPoints = Model.Sum(Model.ForEach(players, i =>
            //    (chooseP[i] + chooseC[i] /*+ choose1B[i] + choose2B[i] + choose3B[i] + chooseSS[i] + chooseLF[i] + chooseCF[i] + chooseRF[i]*/) *
            //    projectedPoints[i])
            //);
            //model.AddGoal("maxPoints", GoalKind.Maximize, sumOfProjectedPoints);

            // Find that lineup
            var simplex = new LpSolveDirective
            {
                TimeLimit = 10000,
                LpSolveMsgFunc = LpSolveMsgFunc,
                LpSolveVerbose = 4,
                LpSolveLogFunc = LpSolveLogFunc
            };
            var solution = context.Solve(simplex);
            Log(solution.GetReport().ToString());
            if (solution.Quality == SolverQuality.Infeasible || solution.Quality == SolverQuality.InfeasibleOrUnbounded ||
                solution.Quality == SolverQuality.Unbounded || solution.Quality == SolverQuality.Unknown)
            {
                Log("No Solution!");
                return null;
            }
            context.PropagateDecisions();
            var lineup = GetSolution(playerPool);
            if (lineup.Count == rosterSlots.Sum(rs => rs.Value))
                return new Solution
                {
                    Lineup = lineup,
                    IsOptimal = solution.Quality == SolverQuality.Optimal
                };

            Log("No Solution!");
            return null;
        }

        private static Term NumPositions(Term i, List<Decision> decisions)
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

        private static void LpSolveLogFunc(int lp, int userhandle, string buffer)
        {
            Log(buffer);
        }

        private static void LpSolveMsgFunc(int lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            Log("Msg: " + message);
        }

        private static Term UniquePosition(Term i, IList<Decision> decisions)
        {
            var decisionCount = decisions.Count;
            var sumOfPositions = new SumTermBuilder(decisionCount);
            for (var j = 0; j < decisionCount; j++)
            {
                sumOfPositions.Add(decisions[j][i]);
            }
            return sumOfPositions.ToTerm() <= 1;
        }

        private static Term AllPlayers(IList<Decision> decisions, IList<Player> playerPool)
        {
            var decisionCount = decisions.Count;
            var sum = new SumTermBuilder(decisionCount);
            for (var i = 0; i < playerPool.Count; i++)
            {
                var sumOfPositions = new SumTermBuilder(decisionCount);
                for (var j = 0; j < decisionCount; j++)
                {
                    sumOfPositions.Add(decisions[j][i]);
                }
                sum.Add(sumOfPositions.ToTerm());
            }
            return sum.ToTerm();
        }

        // decisions are position based, ex. is a pitcher, is a catcher etc.; value 1 if true
        // This creates a term which sums up the value of all the selected players in the player pool
        // based on the current decisions of the solver
        private static Term SumOfSalaries(IList<Decision> decisions, IList<Player> playerPool, Set players)
        {
            var decisionCount = decisions.Count;
            var sum = new SumTermBuilder(playerPool.Count);
            for (var i = 0; i < playerPool.Count; i++)
            {
                var sumOfPositions = new SumTermBuilder(decisionCount);
                for (var j = 0; j < decisionCount; j++)
                {
                    sumOfPositions.Add(decisions[j][i]);
                }
                sum.Add(sumOfPositions.ToTerm() * playerPool[i].Salary);
            }
            return sum.ToTerm();
        }

        private static Term SumOfSalaries(Term i, IList<Decision> decisions, Term salary)
        {
            var decisionCount = decisions.Count;
            var sum = new SumTermBuilder(decisionCount);
            //for (var i = 0; i < playerPool.Count; i++)
            //{
                var sumOfPositions = new SumTermBuilder(decisionCount);
                for (var j = 0; j < decisionCount; j++)
                {
                    sumOfPositions.Add(decisions[j][i]);
                }
                sum.Add(sumOfPositions.ToTerm() * salary);
            //}
            return sum.ToTerm();
        }

        private static Decision CreateAndBindDecision(IEnumerable<Player> playerData, Model model, Set players, string bindingProperty)
        {
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

        private static IList<Player> GetSolution(ICollection<Player> playerPool)
        {
            var selected = playerPool.Where(p => p.Chosen).ToList();

            Log($"Player Pool: {playerPool.Count} total:");
            var totalProjectedPoints = 0;
            var totalSalary = 0;
            foreach (var s in selected)
            {
                totalProjectedPoints += s.ProjectedPoints;
                totalSalary += s.Salary;
                Log(s.ToString());
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
