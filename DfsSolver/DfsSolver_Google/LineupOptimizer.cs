using DfsBase;
using System.Collections.Generic;

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
            return null;
        }
    }
}
