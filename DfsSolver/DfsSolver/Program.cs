// This program uses Microsoft Solver Foundation with lpsolve as the solver library 

using System.Collections.Generic;
using System.Linq;
using DfsBase;

namespace DfsSolver
{
    internal class Program
    {
        // map position id to position name
        public static Position[] MLBPositions = {
            new Position {Id = 10, Name = "P"},
            new Position {Id = 20, Name = "C"},
            new Position {Id = 30, Name = "1B"},
            new Position {Id = 40, Name = "2B"},
            new Position {Id = 50, Name = "3B"},
            new Position {Id = 60, Name = "SS"},
            new Position {Id = 70, Name = "LF"},
            new Position {Id = 80, Name = "CF"},
            new Position {Id = 90, Name = "RF"}
        };

        // map positionId to number of draft positions to fill for that position
        private static readonly IList<LineupSlot> MLBDraftPositions = new List<LineupSlot>
            {
                new LineupSlot{ Name = "P", Count = 2 },
                new LineupSlot{ Name = "C", Count = 1 },
                new LineupSlot{ Name = "1B", Count = 1 },
                new LineupSlot{ Name = "2B", Count = 1 },
                new LineupSlot{ Name = "3B", Count = 1 },
                new LineupSlot{ Name = "SS", Count = 1 },
                new LineupSlot{ Name = "LF", Count = 1 },
                new LineupSlot{ Name = "CF", Count = 1 },
                new LineupSlot{ Name = "RF", Count = 1 },
            };

        public static Position[] NASPositions = {
            new Position {Id = 500, Name = "D"}
        };
        private static readonly IList<LineupSlot> NASDraftPositions = new List<LineupSlot>
        {
            new LineupSlot{ Name = "D", Count = 6 }
        };
        private static void Main()
        {
            LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(MLBPositions, 2).ToList(), MLBDraftPositions, 50000);
            //LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(NASPositions, 7).ToList(), NASDraftPositions, 50000);
        }
    }
}
