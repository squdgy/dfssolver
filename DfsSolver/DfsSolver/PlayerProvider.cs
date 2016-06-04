using System;
using System.Collections.Generic;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static string[] RosterPositions = new string[] { "P", "C", "1B", "2B", "3B", "SS", "LF", "CF", "RF" };

        public static IEnumerable<Player> GetPlayersRandom()
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var rosterPosition in RosterPositions)
            {
                var numPlayers = random.Next(4,15);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = string.Format("Joe_{0} {1}_Player", id, rosterPosition);
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30);
                    players.Add(new Player
                    {
                        Id = id++,
                        Name = name,
                        ProjectedPoints = projectedPoints,
                        Position = rosterPosition,
                        Salary = salary
                    });
                }
            }
            return players;
        }
    }
}
