using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static void Solve(IList<Player> playerData, Dictionary<int, int> rosterSlots, int salaryCap)
        {
            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            var salary = CreateAndBindParameter(playerData, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(playerData, model, players, "ProjectedPoints");
            var pos1 = CreateAndBindParameter(playerData, model, players, "PositionId1");
            var pos2 = CreateAndBindParameter(playerData, model, players, "PositionId2");

            // ---- Define Decisions ----
            // Choose the player or not, 1 if true, and 0 if false
            var choose = CreateAndBindDecision(playerData, model, players, "Chosen");

            // ---- Define Constraints ----
            // reserve the right number to fill the roster
            model.AddConstraint("chosen", Model.Sum(Model.ForEach(players, i => choose[i])) == rosterSlots.Sum(rs => rs.Value));

            // right number at each position.
            foreach (var slot in rosterSlots)
            {
                var pos1Term = Model.ForEachWhere(players, i => choose[i], p => pos1[p] == slot.Key);
                //var pos2Term = Model.ForEachWhere(players, i => choose[i], p => pos2[p] == slot.Key);
                //var posTerm = Model.Or(pos1Term, pos2Term);
                //var posTerm = Model.ForEachWhere(players, i => choose[i], p => pos1[p] == slot.Key || pos2[p] == slot.Key);
                model.AddConstraint("pos_" + slot.Key, Model.Sum(pos1Term) == slot.Value);
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
                Log("No Solution!" + solution.Quality);
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
