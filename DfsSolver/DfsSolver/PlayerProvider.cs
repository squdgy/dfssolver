using System;
using System.Collections.Generic;
using System.Linq;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static IList<Player> GetPlayersRandom(Position[] positions, int minAtPosition)
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var position in positions)
            {
                var numPlayers = random.Next(minAtPosition, minAtPosition + 10);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30);
                    var playerPositions = new HashSet<string> {position.Name};
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                    {
                        var randomPositionIndex = random.Next(0, positions.Length);
                        var position2 = positions[randomPositionIndex];
                        if (!playerPositions.Contains(position2.Name))
                            playerPositions.Add(position2.Name);
                    }
                    players.Add(new Player(positions.Select(pos => pos.Name).ToArray())
                    {
                        Id = id++,
                        Name = name,
                        ProjectedPoints = projectedPoints,
                        Positions = playerPositions,
                        Salary = salary
                    });
                }
            }
            return players;
        }
    }
}
