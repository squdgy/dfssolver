using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static void Solve(IEnumerable<Player> playerData)
        {
            // Constants
            int salaryCap = 50000;
            var rosterSlots = new Dictionary<string, int>
            {
                { "P", 2 },
                { "C", 1 },
                { "1B", 1 },
                { "2B", 1 },
                { "3B", 1 },
                { "SS", 1 },
                { "LF", 1 },
                { "CF", 1 },
                { "RF", 1 }
            };

            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            Parameter salary = CreateAndBindParameter(playerData, model, players, "Salary");
            Parameter projectedPoints = CreateAndBindParameter(playerData, model, players, "ProjectedPoints");

            // Positions
            Parameter pitcher = CreateAndBindParameter(playerData, model, players, "IsPitcherVal");
            Parameter catcher = CreateAndBindParameter(playerData, model, players, "IsCatcherVal");
            Parameter firstBaseman = CreateAndBindParameter(playerData, model, players, "Is1BVal");
            Parameter secondBaseman = CreateAndBindParameter(playerData, model, players, "Is2BVal");
            Parameter thirdBaseman = CreateAndBindParameter(playerData, model, players, "Is3BVal");
            Parameter shortstop = CreateAndBindParameter(playerData, model, players, "IsSSVal");
            Parameter leftFielders = CreateAndBindParameter(playerData, model, players, "IsLFVal");
            Parameter centerFielders = CreateAndBindParameter(playerData, model, players, "IsCFVal");
            Parameter rightFielders = CreateAndBindParameter(playerData, model, players, "IsRFVal");

            // ---- Define Decisions ----
            // For each position, if player is choosen for that position, 1 if true, and 0 if false
            Decision chooseP = CreateAndBindDecision(playerData, model, players, "ChosenPitcher");
            Decision chooseC = CreateAndBindDecision(playerData, model, players, "ChosenCatcher");
            Decision choose1B = CreateAndBindDecision(playerData, model, players, "Chosen1B");
            Decision choose2B = CreateAndBindDecision(playerData, model, players, "Chosen2B");
            Decision choose3B = CreateAndBindDecision(playerData, model, players, "Chosen3B");
            Decision chooseSS = CreateAndBindDecision(playerData, model, players, "ChosenSS");
            Decision chooseLF = CreateAndBindDecision(playerData, model, players, "ChosenLF");
            Decision chooseCF = CreateAndBindDecision(playerData, model, players, "ChosenCF");
            Decision chooseRF = CreateAndBindDecision(playerData, model, players, "ChosenRF");

            // ---- Define Constraints ----
            // Reserve the right number of each position.
            model.AddConstraint("pitchers", Model.Sum(Model.ForEach(players, i => chooseP[i] * pitcher[i])) == rosterSlots["P"]);
            model.AddConstraint("catchers", Model.Sum(Model.ForEach(players, i => chooseC[i] * catcher[i])) == rosterSlots["C"]);
            model.AddConstraint("first_basemen", Model.Sum(Model.ForEach(players, i => choose1B[i] * firstBaseman[i])) == rosterSlots["1B"]);
            model.AddConstraint("second_basemen", Model.Sum(Model.ForEach(players, i => choose2B[i] * secondBaseman[i])) == rosterSlots["2B"]);
            model.AddConstraint("third_basemen", Model.Sum(Model.ForEach(players, i => choose3B[i] * thirdBaseman[i])) == rosterSlots["3B"]);
            model.AddConstraint("shortstop", Model.Sum(Model.ForEach(players, i => chooseSS[i] * shortstop[i])) == rosterSlots["SS"]);
            model.AddConstraint("leftFielders", Model.Sum(Model.ForEach(players, i => chooseLF[i] * leftFielders[i])) == rosterSlots["LF"]);
            model.AddConstraint("centerFielders", Model.Sum(Model.ForEach(players, i => chooseCF[i] * centerFielders[i])) == rosterSlots["CF"]);
            model.AddConstraint("rightFielders", Model.Sum(Model.ForEach(players, i => chooseRF[i] * rightFielders[i])) == rosterSlots["RF"]);

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
            var solution = context.Solve();
            Log(solution.GetReport().ToString());
            if (solution.Quality == SolverQuality.Infeasible ||
                solution.Quality == SolverQuality.InfeasibleOrUnbounded)
            {
                Log("No Solution!");
                return;
            }
            context.PropagateDecisions();
            ReportSolution(playerData, solution);
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

        private static void ReportSolution(IEnumerable<Player> playerData, Solution solution)
        {
            var selected = playerData.Where(p => p.Chosen);

            Log(string.Format("Player Pool: {0} total:", playerData.Count()));
            var groupings = playerData.GroupBy(p => p.Position);
            foreach (var grouping in groupings)
            {
                Log(string.Format("{0} {1}", grouping.Key, grouping.Count()));
            }

            var totalProjectedPoints = 0;
            var totalSalary = 0;
            foreach (var s in selected)
            {
                totalProjectedPoints += s.ProjectedPoints;
                totalSalary += s.Salary;
                Log(s.ToString());
            }
            Log(string.Format("Projected Points: {0}, Used Salary: {1}", totalProjectedPoints, totalSalary));
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
