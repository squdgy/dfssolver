using System;
using System.Collections.Generic;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static Position[] RosterPositions = new Position[]
        {
            new Position {Id = 1, Name = "P"},
            new Position {Id = 2, Name = "C"},
            new Position {Id = 3, Name = "1B"},
            new Position {Id = 4, Name = "2B"},
            new Position {Id = 5, Name = "3B"},
            new Position {Id = 6, Name = "SS"},
            new Position {Id = 7, Name = "LF"},
            new Position {Id = 8, Name = "CF"},
        };

        public static IEnumerable<Player> GetPlayersRandom()
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var rosterPosition in RosterPositions)
            {
                var numPlayers = random.Next(4,6);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30);
                    var positions = new HashSet<string>();
                    positions.Add(rosterPosition.Name);
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                        positions.Add(RosterPositions[random.Next(0, 9)].Name);
                    players.Add(new Player
                    {
                        Id = id++,
                        Name = name,
                        ProjectedPoints = projectedPoints,
                        Positions = positions,
                        Salary = salary
                    });
                }
            }
            return players;
        }
    }
}
