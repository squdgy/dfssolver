﻿using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        private readonly int _timeoutInMilliseconds;

        /// <summary>
        /// Whether or not the returned solution was optimal or a result of a timeout
        /// </summary>
        public bool IsOptimal { get; set; }

        /// <summary>
        /// Create a lineup optimizer object
        /// </summary>
        /// <param name="timeOutInMilliseconds">If optimal solution not found after this timeout, find the best one so far</param>
        public LineupOptimizer(int timeOutInMilliseconds = 30000)
        {
            _timeoutInMilliseconds = timeOutInMilliseconds;
        }

        /// <summary>
        /// Returns list of selected players
        /// </summary>
        /// <param name="playerPool">all possible players, preselected ones set with the DraftPositionId != 0</param>
        /// <param name="lineupSlots">dictionary of position ids to 3 of players allowed in that position</param>
        /// <param name="salaryCap">maximum amount to spend</param>
        /// <returns>The players that are selected by the solver</returns>
        public IList<Player> Solve(IList<Player> playerPool, Dictionary<int, int> lineupSlots, int salaryCap)
        {
            // deal with pre filled slots
            var prefilled = playerPool.Where(p => p.IsDrafted).ToList();
            var unfilledCap = salaryCap - prefilled.Sum(p => p.Salary);
            var unfilledSlots = new Dictionary<int, int>();
            foreach (var slot in lineupSlots)
            {
                var prefilledCount = prefilled.Count(p => p.DraftPositionId == slot.Key);
                var newCountForSlot = slot.Value - prefilledCount;
                if (newCountForSlot > 0) unfilledSlots.Add(slot.Key, newCountForSlot);
            }
            var unfilledPosIds = unfilledSlots.Select(ls => ls.Key).ToList();

            // trim the draft pool by removing prefilled players
            // and those whose draft positions don't need to be filled
            var availablePlayers = playerPool.Except(prefilled).Where(
                    p => unfilledPosIds.Contains(p.PositionId1) || unfilledPosIds.Contains(p.PositionId2)).ToList();

            var context = SolverContext.GetContext();
            var model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // ---- Define Parameters ----
            var salary = CreateAndBindParameter(availablePlayers, model, players, "Salary");
            var projectedPoints = CreateAndBindParameter(availablePlayers, model, players, "ProjectedPointsAsInt");
            var position1 = CreateAndBindParameter(availablePlayers, model, players, "PositionId1");
            var position2 = CreateAndBindParameter(availablePlayers, model, players, "PositionId2");

            // ---- Define Decisions ----
            // Choose the selected position id for the player; 0 implies not drafted
            var positionIds = unfilledSlots.Select(rs => rs.Key).ToList();
            positionIds.Add(0);
            var chooseP = new Decision(Domain.Set(positionIds.ToArray()), "DraftPositionId", players);
            chooseP.SetBinding(availablePlayers, "DraftPositionId", "Id");
            model.AddDecision(chooseP);

            // ---- Define Constraints ----
            // reserve the right number to fill the lineup
            var lineupSize = unfilledSlots.Sum(rs => rs.Value);
            Func<Term, Term> isDrafted = i => Model.If(chooseP[i] > 0, 1, 0);
            model.AddConstraint("drafted", Model.Sum(Model.ForEach(players, isDrafted)) == lineupSize);

            // right number at each position.
            foreach (var slot in unfilledSlots)
            {
                var positionId = slot.Key;
                var positionCount = slot.Value;
                Func<Term, Term> isDraftedValues = i => Model.If(chooseP[i] == positionId, 1, 0);
                Func<Term, Term> isEligibleAtPosition = p => position2[p] == positionId | position1[p] == positionId;
                var pos1Term = Model.ForEachWhere(players, isDraftedValues, isEligibleAtPosition);
                model.AddConstraint("pos_" + positionId, Model.Sum(pos1Term) == positionCount);
            }

            // within the salary cap
            model.AddConstraint("withinSalaryCap", Model.Sum(Model.ForEach(players, i => Model.If(chooseP[i] > 0, salary[i], 0))) <= unfilledCap);

            // ---- Define the Goal ----
            // maximize for projectedPoints
            model.AddGoal("maxPoints", GoalKind.Maximize, Model.Sum(Model.ForEach(players, i => Model.If(chooseP[i] > 0, projectedPoints[i], 0))));

            // Find that lineup
            var directive = new Directive
            {
                TimeLimit = _timeoutInMilliseconds
            };
            var solution = context.Solve(directive);
            IsOptimal = solution.Quality == 
                SolverQuality.LocalOptimal || solution.Quality == SolverQuality.Optimal;

            Log(solution.GetReport().ToString());
            if (solution.Quality == SolverQuality.Infeasible ||
                solution.Quality == SolverQuality.InfeasibleOrUnbounded)
            {
                Log("No Solution!" + solution.Quality);
                return null;
            }
            context.PropagateDecisions();
            return ReportSolution(playerPool, prefilled, availablePlayers);
        }

        private static Parameter CreateAndBindParameter(IEnumerable<Player> playerData, Model model, Set players, string bindingProperty)
        {
            var param = new Parameter(Domain.Integer, bindingProperty, players);
            param.SetBinding(playerData, bindingProperty, "Id");
            model.AddParameter(param);
            return param;
        }

        private static IList<Player> ReportSolution(ICollection<Player> playerPool, ICollection<Player> prefilled, ICollection<Player> available)
        {
            Log("====================================================");
            Log($"Pre-filled players: {prefilled.Count}");
            Log("=========================================");
            foreach (var s in prefilled)
            {
                Log(s.ToString());
            }
            Log("=========================================");

            var selected = prefilled.Union(playerPool.Where(p => p.IsDrafted)).OrderBy(p => p.DraftPositionId);
            Log($"Player Pool Size: {playerPool.Count}, After prefill exclusions: {available.Count}");
            Log("====================================================");
            Log("Lineup");
            Log("======");
            var totalProjectedPoints = 0m;
            var totalSalary = 0;
            foreach (var s in selected)
            {
                totalProjectedPoints += s.ProjectedPoints;
                totalSalary += s.Salary;
                Log(s.ToString());
            }
            Log("=========================================");
            Log($"Projected Points: {totalProjectedPoints}, Used Salary: {totalSalary}");
            Log("====================================================");

            return selected.ToList();
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
