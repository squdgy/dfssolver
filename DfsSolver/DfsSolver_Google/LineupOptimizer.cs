using DfsBase;
using Google.OrTools.ConstraintSolver;
using System.Collections.Generic;
using System.Linq;

namespace DfsSolver_Google
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
            var unfilledSlots = new List<LineupSlot>();
            foreach (var slot in lineupSlots)
            {
                var prefilledCount = prefilled.Count(p => p.ChosenPosition == slot.Name);
                var newCountForSlot = slot.Count - prefilledCount;
                unfilledSlots.Add(new LineupSlot
                {
                    Name = slot.Name,
                    Count = newCountForSlot
                });
            }

            // trim the draft pool by removing those whose draft positions don't need to be filled
            var unfilledPosNames = unfilledSlots.Where(ls => ls.Count > 0).Select(ls => ls.Name).ToList();
            availablePlayers = availablePlayers.Where(
                    p => unfilledPosNames.Intersect(p.Positions).Any()).ToList();

            // TODO: Configure Solver
            var solver = new Solver("LineupOptimizer");

            // TODO: Define Parameters for data
            // players with:
            // - name/id, salary, game, team, list of positions
            // TODO: Define Decision Variables
            // - roster with n spots where n is number of spots needed to fill
            // TODO: Create Constraints
            // - max salary
            // - max per position
            // - max per team
            // - min games
            // - min teams
            // TODO: Create the goal
            // - maximize projected fantasy points
            // TODO: Solve
            // TODO: Report solution(s)
            return null;
        }
    }
}
