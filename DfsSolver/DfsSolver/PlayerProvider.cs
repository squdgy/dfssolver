using System;
using System.Collections.Generic;
using System.Linq;

namespace DfsSolver
{
    public class PlayerProvider
    {
        public static IEnumerable<Player> GetPlayersRandom(List<Position> positions)
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var position1 in positions)
            {
                var numPlayers = random.Next(6,7);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    decimal projectedPoints = random.Next(0, 30) + ((decimal)random.Next(0,99))/100;
                    var playerPositions = new List<Position> {position1};
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                    {
                        var randomPositionIndex = random.Next(0, positions.Count);
                        var position2 = positions[randomPositionIndex];
                        if (position2.Id != position1.Id)
                            playerPositions.Add(position2);
                    }
                    players.Add(new Player
                    {
                        Id = id++,
                        Name = name,
                        ProjectedPoints = projectedPoints,
                        Positions = playerPositions,
                        Salary = salary
                    });
                }
            }

            // randomly prefill n players
            var n = random.Next(1, positions.Count + 1);
            for (var i = 0; i < n; i++)
            {
                var position = positions[i];
                var eligiblePlayers = players.Where(p => p.Positions.Contains(position));
                eligiblePlayers.First().DraftPositionId = position.Id;
            }

            return players;
        }
    }
}
