using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static void Solve(IList<Player> playerData, Dictionary<int, int> lineupSlots, int salaryCap)
        {
            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            var salary = CreateAndBindParameter(playerData, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(playerData, model, players, "ProjectedPoints");
            var position1 = CreateAndBindParameter(playerData, model, players, "PositionId1");
            var position2 = CreateAndBindParameter(playerData, model, players, "PositionId2");
            var drafted = CreateAndBindParameter(playerData, model, players, "IsDrafted");

            // ---- Define Decisions ----
            // Choose the selected position id for the player; 0 implies not drafted
            var posIds = lineupSlots.Select(rs => rs.Key).ToList();
            posIds.Add(0);
            var chooseP = new Decision(Domain.Set(posIds.ToArray()), "DraftPositionId", players);
            chooseP.SetBinding(playerData, "DraftPositionId", "Id");
            model.AddDecision(chooseP);

            // ---- Define Constraints ----
            // reserve the right number to fill the lineup
            var lineupSize = lineupSlots.Sum(rs => rs.Value);
            Func<Term, Term> isDrafted = i => Model.If(chooseP[i] > 0, 1, 0);
            model.AddConstraint("drafted", Model.Sum(Model.ForEach(players, isDrafted)) == lineupSize);

            // right number at each position.
            foreach (var slot in lineupSlots)
            {
                var positionId = slot.Key;
                var positionCount = slot.Value;
                Func<Term, Term> isDraftedValues = i => Model.If(chooseP[i] == positionId, 1, 0);
                Func<Term, Term> isEligibleAtPosition = p => position2[p] == positionId | position1[p] == positionId;
                var pos1Term = Model.ForEachWhere(players, isDraftedValues, isEligibleAtPosition);
                model.AddConstraint("pos_" + positionId, Model.Sum(pos1Term) == positionCount);
            }

            // within the salary cap
            model.AddConstraint("withinSalaryCap", Model.Sum(Model.ForEach(players, i => drafted[i] * salary[i])) <= salaryCap);

            // ---- Define the Goal ----
            // maximize for projectedPoints
            model.AddGoal("maxPoints", GoalKind.Maximize, Model.Sum(Model.ForEach(players, i => drafted[i] * projectedPoints[i])));

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

        private static Parameter CreateAndBindParameter(IEnumerable<Player> playerData, Model model, Set players, string bindingProperty)
        {
            var param = new Parameter(Domain.Integer, bindingProperty, players);
            param.SetBinding(playerData, bindingProperty, "Id");
            model.AddParameter(param);
            return param;
        }

        private static void ReportSolution(ICollection<Player> playerData)
        {
            var selected = playerData.Where(p => p.IsDrafted).OrderBy(p => p.DraftPositionId);

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
