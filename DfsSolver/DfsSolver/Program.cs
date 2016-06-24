// 

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
        private static readonly Dictionary<string, int> MLBDraftPositions = new Dictionary<string, int>
            {
                { "P", 2 }, //P
                { "C", 1 }, //C
                { "1B", 1 }, //1B
                //{ "2B", 1 }, //2B
                //{ "3B", 1 }, //3B
                //{ "SS", 1 }, //SS
                //{ "LF", 1 }, //LF
                //{ "CF", 1 }, //CF
                //{ "RF", 1 }  //RF
            };

        public static Position[] NASPositions = {
            new Position {Id = 500, Name = "D"}
        };
        private static readonly Dictionary<string, int> NASDraftPositions = new Dictionary<string, int>
        {
            {"D", 6}
        };
        private static void Main()
        {
            LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(MLBPositions, 4).ToList(), MLBDraftPositions, 50000);
            //LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom(NASPositions, 7).ToList(), NASDraftPositions, 50000);
        }
    }
}
