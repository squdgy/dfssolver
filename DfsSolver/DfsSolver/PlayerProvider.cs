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

            // randomly prefill n players
            var n = random.Next(1, positions.Length + 1);
            for (var i = 0; i < n; i++)
            {
                var position = positions[i];
                var eligiblePlayers = players.Where(p => p.Positions.Contains(position.Name)).ToList();
                var eligiblePlayerCount = eligiblePlayers.Count;
                var randEligible = random.Next(0, eligiblePlayerCount);
                var player = eligiblePlayers[randEligible];
                if (player.Chosen) continue; // don't re-choose player for another position
                switch (i)
                {
                    case 0:
                        player.ChosenAtPosition0 = 1;
                        break;
                    case 1:
                        player.ChosenAtPosition1 = 1;
                        break;
                    case 2:
                        player.ChosenAtPosition2 = 1;
                        break;
                    case 3:
                        player.ChosenAtPosition3 = 1;
                        break;
                    case 4:
                        player.ChosenAtPosition4 = 1;
                        break;
                    case 5:
                        player.ChosenAtPosition5 = 1;
                        break;
                    case 6:
                        player.ChosenAtPosition6 = 1;
                        break;
                    case 7:
                        player.ChosenAtPosition7 = 1;
                        break;
                    case 8:
                        player.ChosenAtPosition8 = 1;
                        break;
                }
            }

            return players;
        }
    }
}
