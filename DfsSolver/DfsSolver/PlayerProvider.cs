using System;
using System.Collections.Generic;
using System.Linq;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static IEnumerable<Player> GetPlayersRandom(Position[] mlbPositions)
        {
            var rosterPositions = mlbPositions.Select(pos => pos.Name).ToList();

            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var rosterPosition in rosterPositions)
            {
                var numPlayers = random.Next(4,15);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30);
                    var positions = new HashSet<string> {rosterPosition};
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                        positions.Add(rosterPositions[random.Next(0, 9)]);
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
