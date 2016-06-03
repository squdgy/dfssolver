using System;
using System.Collections.Generic;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static string[] RosterPositions = new string[] { "P", "C" };

        public static IEnumerable<Player> GetPlayersRandom()
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var rosterPosition in RosterPositions)
            {
                var numPlayers = random.Next(4, 10);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = string.Format("Joe_{0} Player", id);
                    var salary = random.Next(2000, 5000);
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

        public static IEnumerable<Player> GetPlayers()
        {
            var players = new List<Player>();
            players.Add(new Player
            {
                Id = 1,
                Name = "Joe Schmoe",
                ProjectedPoints = 23,
                Position = "P",
                Salary = 4200
            });
            players.Add(new Player
            {
                Id = 2,
                Name = "Jack Flack",
                ProjectedPoints = 18,
                Position = "P",
                Salary = 2800
            });
            players.Add(new Player
            {
                Id = 3,
                Name = "Carl Snarl",
                ProjectedPoints = 11,
                Position = "P",
                Salary = 2400
            });
            players.Add(new Player
            {
                Id = 4,
                Name = "Buster Guster",
                ProjectedPoints = 12,
                Position = "C",
                Salary = 3800
            });
            players.Add(new Player
            {
                Id = 5,
                Name = "Frank Jank",
                ProjectedPoints = 10,
                Position = "C",
                Salary = 3000
            });
            players.Add(new Player
            {
                Id = 6,
                Name = "Theo Leo",
                ProjectedPoints = 11,
                Position = "C",
                Salary = 2400
            });
            return players;
        }
    }
}
