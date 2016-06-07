using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static void Solve(IList<Player> playerData)
        {
            // Constants
            const int salaryCap = 50000;
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
            var positionParameters = new Dictionary<string, Parameter>();
            positionParameters.Add("P", CreateAndBindParameter(playerData, model, players, "IsPitcherVal"));
            positionParameters.Add("C", CreateAndBindParameter(playerData, model, players, "IsCatcherVal"));
            positionParameters.Add("1B", CreateAndBindParameter(playerData, model, players, "Is1BVal"));
            positionParameters.Add("2B", CreateAndBindParameter(playerData, model, players, "Is2BVal"));
            positionParameters.Add("3B", CreateAndBindParameter(playerData, model, players, "Is3BVal"));
            positionParameters.Add("SS", CreateAndBindParameter(playerData, model, players, "IsSSVal"));
            positionParameters.Add("LF", CreateAndBindParameter(playerData, model, players, "IsLFVal"));
            positionParameters.Add("CF", CreateAndBindParameter(playerData, model, players, "IsCFVal"));
            positionParameters.Add("RF", CreateAndBindParameter(playerData, model, players, "IsRFVal"));

            // ---- Define Decisions ----
            // Choose the player or not, 1 if true, and 0 if false
            var choose = CreateAndBindDecision(playerData, model, players, "Chosen");

            // ---- Define Constraints ----
            // reserve the right number to fill the roster
            model.AddConstraint("chosen", Model.Sum(Model.ForEach(players, i => choose[i] * choose[i])) == rosterSlots.Sum(rs => rs.Value));

            // right number at each position.
            foreach (var slot in rosterSlots)
            {
                var posParam = positionParameters[slot.Key];
                var posTerm = Model.ForEachWhere(players, i => choose[i], p => posParam[p] == 1);
                model.AddConstraint("pos_" + slot.Key, Model.Sum(posTerm) == slot.Value);
            }

            // within the salary cap
            model.AddConstraint("maxSalary", Model.Sum(Model.ForEach(players, i => choose[i] * salary[i])) <= salaryCap);

            // ---- Define the Goal ----
            // maximize for projectedPoints
            model.AddGoal("maxPoints", GoalKind.Maximize, Model.Sum(Model.ForEach(players, i => choose[i] * projectedPoints[i])));

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
            ReportSolution(playerData);
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

        private static void ReportSolution(IList<Player> playerData)
        {
            var selected = playerData.Where(p => p.IsChosen);

            Log($"Player Pool: {playerData.Count} total:");
            var totalProjectedPoints = 0;
            var totalSalary = 0;
            foreach (var s in selected)
            {
                totalProjectedPoints += s.ProjectedPoints;
                totalSalary += s.Salary;
                Log(s.ToString());
            }
            Log($"Projected Points: {totalProjectedPoints}, Used Salary: {totalSalary}");
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
