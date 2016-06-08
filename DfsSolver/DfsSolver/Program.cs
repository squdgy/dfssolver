using System.Collections.Generic;
using System.Linq;

namespace DfsSolver
{
    internal class Program
    {
        // map position id to position name
        public static Position[] Positions = {
            new Position {Id = 1, Name = "P"},
            new Position {Id = 2, Name = "C"},
            new Position {Id = 3, Name = "1B"},
            new Position {Id = 4, Name = "2B"},
            new Position {Id = 5, Name = "3B"},
            new Position {Id = 6, Name = "SS"},
            new Position {Id = 7, Name = "LF"},
            new Position {Id = 8, Name = "CF"},
            new Position {Id = 9, Name = "RF"}
        };

        // map positionId to number of draft positions to fill for that position
        private static readonly Dictionary<int, int> DraftPositions = new Dictionary<int, int>
            {
                { 1, 2 }, //P
                { 2, 1 }, //C
                { 3, 1 }, //1B
                { 4, 1 }, //2B
                { 5, 1 }, //3B
                { 6, 1 }, //SS
                { 7, 1 }, //LF
                { 8, 1 }, //CF
                { 9, 1 }  //RF
            };


        private static void Main()
        {
            var positions = Positions.ToList();
            var playerPool = PlayerProvider.GetPlayersRandom(positions).ToList();
            LineupOptimizer.Solve(playerPool.ToList(), DraftPositions, 50000);
        }
    }
}
