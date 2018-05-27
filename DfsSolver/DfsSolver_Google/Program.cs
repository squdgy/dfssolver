// This program uses Google Optimization Tools (OR-tools) and the GLOP solver

using System;
using System.Collections.Generic;
using System.Linq;
using DfsBase;

namespace DfsSolver_Google
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

        private static ContestRules MLBRules1 = new ContestRules
        {
            SalaryCap = 50000,
            MinGames = 5,
            MinTeams = 3,
            MaxPerTeam = 4
        };

        public static Position[] NASPositions = {
            new Position {Id = 500, Name = "D"}
        };
        private static readonly IList<LineupSlot> NASDraftPositions = new List<LineupSlot>
        {
            new LineupSlot{ Name = "D", Count = 6 }
        };
        private static ContestRules NASRules1 = new ContestRules
        {
            SalaryCap = 50000,
            MinGames = 1,
            MinTeams = 1,
            MaxPerTeam = -1
        };


        private static void Main()
        {
            LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(MLBPositions, 100, 15, 2).ToList(), MLBDraftPositions, MLBRules1);
            //LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(NASPositions, 7, 1, 1).ToList(), NASDraftPositions, NASRules1);
        }
    }
}
