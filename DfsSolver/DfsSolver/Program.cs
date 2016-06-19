using System.Collections.Generic;
using System.Linq;

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
        private static readonly Dictionary<int, int> MLBDraftPositions = new Dictionary<int, int>
            {
                { 10, 2 }, //P
                { 20, 1 }, //C
                { 30, 1 }, //1B
                { 40, 1 }, //2B
                { 50, 1 }, //3B
                { 60, 1 }, //SS
                { 70, 1 }, //LF
                { 80, 1 }, //CF
                { 90, 1 }  //RF
            };

        public static Position[] NASPositions = {
            new Position {Id = 400, Name = "D"}
        };
        private static readonly Dictionary<int, int> NASDraftPositions = new Dictionary<int, int>
        {
            {400, 6}
        };


        private static void Main()
        {
            var sport = "MLB";
            var lineupOptimizer = new LineupOptimizer();
            switch (sport)
            {
                case "MLB":
                    lineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(MLBPositions.ToList()).ToList(), MLBDraftPositions, 50000);
                    return;
                case "NAS":
                    lineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(NASPositions.ToList()).ToList(), NASDraftPositions, 50000);
                    return;
            }
        }
    }
}
