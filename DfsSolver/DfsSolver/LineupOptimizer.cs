using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SolverFoundation.Plugin.LpSolve;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static Solution Solve(IList<Player> playerPool, Dictionary<string, int> rosterSlots, int salaryCap)
        {
            var slotList = rosterSlots.ToList();
            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            var salary = CreateAndBindParameter(playerPool, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(playerPool, model, players, "ProjectedPoints");

            // Positions
            var pitcher = CreateAndBindParameter(playerPool, model, players, "IsPitcherVal");
            var catcher = CreateAndBindParameter(playerPool, model, players, "IsCatcherVal");
            var firstBaseman = CreateAndBindParameter(playerPool, model, players, "Is1BVal");
            var secondBaseman = CreateAndBindParameter(playerPool, model, players, "Is2BVal");
            var thirdBaseman = CreateAndBindParameter(playerPool, model, players, "Is3BVal");
            var shortstop = CreateAndBindParameter(playerPool, model, players, "IsSSVal");
            var leftFielders = CreateAndBindParameter(playerPool, model, players, "IsLFVal");
            var centerFielders = CreateAndBindParameter(playerPool, model, players, "IsCFVal");
            var rightFielders = CreateAndBindParameter(playerPool, model, players, "IsRFVal");

            // ---- Define Decisions ----
            // For each position, if player is choosen for that position, 1 if true, and 0 if false
            var chooseP = CreateAndBindDecision(playerPool, model, players, "ChosenPitcher");
            var chooseC = CreateAndBindDecision(playerPool, model, players, "ChosenCatcher");
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
            model.AddConstraint("pitchers", Model.Sum(Model.ForEach(players, i => chooseP[i] * pitcher[i])) == slotList[0].Value);
            //var cc = slotList.Count > 1 ? slotList[1].Value : 0;
            model.AddConstraint("catchers", Model.Sum(Model.ForEach(players, i => chooseC[i] * catcher[i])) == slotList[1].Value);
            //model.AddConstraint("first_basemen", Model.Sum(Model.ForEach(players, i => choose1B[i] * firstBaseman[i])) == rosterSlots["1B"]);
            //model.AddConstraint("second_basemen", Model.Sum(Model.ForEach(players, i => choose2B[i] * secondBaseman[i])) == rosterSlots["2B"]);
            //model.AddConstraint("third_basemen", Model.Sum(Model.ForEach(players, i => choose3B[i] * thirdBaseman[i])) == rosterSlots["3B"]);
            //model.AddConstraint("shortstop", Model.Sum(Model.ForEach(players, i => chooseSS[i] * shortstop[i])) == rosterSlots["SS"]);
            //model.AddConstraint("leftFielders", Model.Sum(Model.ForEach(players, i => chooseLF[i] * leftFielders[i])) == rosterSlots["LF"]);
            //model.AddConstraint("centerFielders", Model.Sum(Model.ForEach(players, i => chooseCF[i] * centerFielders[i])) == rosterSlots["CF"]);
            //model.AddConstraint("rightFielders", Model.Sum(Model.ForEach(players, i => chooseRF[i] * rightFielders[i])) == rosterSlots["RF"]);

            // same player can't be in 2 roster spots at the same time
            var decisions = new List<Decision> {chooseP, chooseC};
            //var decisionCount = 2;
            //var playerCount = playerPool.Count;
            //var sumOfUniqueLineupPositions = new SumTermBuilder(decisionCount);

            //for (var j = 0; j < decisionCount; j++)
            //{
            //    sumOfUniqueLineupPositions.Add(decisions[j]);
            //}
 //           model.AddConstraints("noMoreThanOnce", Model.ForEach(players, i => UniqueLineupPositionFunc(i, decisions, playerCount)));

            // within the salary cap
            var sumOfSalaries = SumOfSalaries(decisions, playerPool);
            //    Model.Sum(Model.ForEach(players, i => WithinSalaryTerm(i, decisions, salary, playerPool.Count)
            //    (chooseP[i] + chooseC[i] /*+ choose1B[i] + choose2B[i] + choose3B[i] + chooseSS[i] + chooseLF[i] + chooseCF[i] + chooseRF[i]*/) *
            //    salary[i])
            //);
            model.AddConstraint("maxSalary", sumOfSalaries <= salaryCap);

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
                TimeLimit = 10000
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

        // decisions are position based, ex. is a pitcher, is a catcher etc.; value 1 if true
        private static Term SumOfSalaries(IList<Decision> decisions, IList<Player> playerPool)
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
                sum.Add(sumOfPositions.ToTerm() * playerPool[i].Salary);
            }
            return sum.ToTerm();
        }

        private static Term UniqueLineupPositionFunc(Term term, List<Decision> decisions, int numPlayers)
        {
            //var uniqueTermBuilder = new SumTermBuilder(decisions.Count);
            //foreach (var decision in decisions)
            //{
            //    uniqueTermBuilder.Add(decision.GetDouble());
            //}
            ////return i => decisions.Aggregate<Decision, Term>(0, (current, t) => current + t);
            //return i => uniqueTermBuilder.ToTerm();
            var decisionCount = decisions.Count;
            var numDecisionValues = numPlayers;
            var allTerms = new List<Term>();
            //foreach (var decision in decisions[0].Binding)
            //{
            //    var sumOfUniqueLineupPositions = new SumTermBuilder(decisionCount);
            //    for (var j = 0; j < decisionCount; j++)
            //    {
            //        sumOfUniqueLineupPositions.Add(decisions[j]);
            //    }
            //    allTerms.Add(sumOfUniqueLineupPositions.ToTerm() <= 1);
            //}
            for (var i = 0; i < numDecisionValues; i++ )
            {
                var sumOfUniqueLineupPositions = new SumTermBuilder(decisionCount);
                for (var j = 0; j < decisionCount; j++)
                {
                    sumOfUniqueLineupPositions.Add(decisions[j][i]);
                }
                allTerms.Add(sumOfUniqueLineupPositions.ToTerm() <= 1);
            }
            return Model.And(allTerms.ToArray());
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
