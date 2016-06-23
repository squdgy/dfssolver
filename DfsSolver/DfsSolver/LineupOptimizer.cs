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
        public static Solution Solve(IList<Player> playerPool, Dictionary<string, int> lineupSlots, int salaryCap)
        {

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
            var choose1B = CreateAndBindDecision(playerPool, model, players, "Chosen1B");
            var choose2B = CreateAndBindDecision(playerPool, model, players, "Chosen2B");
            var choose3B = CreateAndBindDecision(playerPool, model, players, "Chosen3B");
            var chooseSS = CreateAndBindDecision(playerPool, model, players, "ChosenSS");
            var chooseLF = CreateAndBindDecision(playerPool, model, players, "ChosenLF");
            var chooseCF = CreateAndBindDecision(playerPool, model, players, "ChosenCF");
            var chooseRF = CreateAndBindDecision(playerPool, model, players, "ChosenRF");

            // ---- Define Constraints ----
            // Reserve the right number of each position.
            model.AddConstraint("pitchers", Model.Sum(Model.ForEach(players, i => chooseP[i] * pitcher[i])) == lineupSlots["P"]);
            model.AddConstraint("catchers", Model.Sum(Model.ForEach(players, i => chooseC[i] * catcher[i])) == lineupSlots["C"]);
            model.AddConstraint("first_basemen", Model.Sum(Model.ForEach(players, i => choose1B[i] * firstBaseman[i])) == lineupSlots["1B"]);
            model.AddConstraint("second_basemen", Model.Sum(Model.ForEach(players, i => choose2B[i] * secondBaseman[i])) == lineupSlots["2B"]);
            model.AddConstraint("third_basemen", Model.Sum(Model.ForEach(players, i => choose3B[i] * thirdBaseman[i])) == lineupSlots["3B"]);
            model.AddConstraint("shortstop", Model.Sum(Model.ForEach(players, i => chooseSS[i] * shortstop[i])) == lineupSlots["SS"]);
            model.AddConstraint("leftFielders", Model.Sum(Model.ForEach(players, i => chooseLF[i] * leftFielders[i])) == lineupSlots["LF"]);
            model.AddConstraint("centerFielders", Model.Sum(Model.ForEach(players, i => chooseCF[i] * centerFielders[i])) == lineupSlots["CF"]);
            model.AddConstraint("rightFielders", Model.Sum(Model.ForEach(players, i => chooseRF[i] * rightFielders[i])) == lineupSlots["RF"]);

            // same player can't be in 2 roster spots at the same time
            model.AddConstraints("noMoreThanOnce", Model.ForEach(players, i =>
                chooseP[i] + chooseC[i] + choose1B[i] + choose2B[i] + choose3B[i] + chooseSS[i] + chooseLF[i] + chooseCF[i] + chooseRF[i]
                 <= 1));

            // within the salary cap
            var sumOfSalaries = Model.Sum(Model.ForEach(players, i =>
                (chooseP[i] + chooseC[i] + choose1B[i] + choose2B[i] + choose3B[i] + chooseSS[i] + chooseLF[i] + chooseCF[i] + chooseRF[i]) *
                salary[i])
            );
            model.AddConstraint("maxSalary", sumOfSalaries <= salaryCap);

            // ---- Define the Goal ----
            // maximize for projectedPoints
            var sumOfProjectedPoints = Model.Sum(Model.ForEach(players, i =>
                (chooseP[i] + chooseC[i] + choose1B[i] + choose2B[i] + choose3B[i] + chooseSS[i] + chooseLF[i] + chooseCF[i] + chooseRF[i]) *
                projectedPoints[i])
            );
            model.AddGoal("maxPoints", GoalKind.Maximize, sumOfProjectedPoints);

            // Find that lineup
            var simplex = new LpSolveDirective
            {
                TimeLimit = 10000
            };
            var solution = context.Solve(simplex);
            Log(solution.GetReport().ToString());
            context.PropagateDecisions();
            var lineup = GetSolution(playerPool);
            if (lineup.Count == lineupSlots.Sum(rs => rs.Value))
                return new Solution
                {
                    Lineup = lineup,
                    IsOptimal = solution.Quality == SolverQuality.Optimal
                };

            Log("No Solution!");
            return null;
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
