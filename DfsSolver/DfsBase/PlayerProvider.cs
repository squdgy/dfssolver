﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DfsBase
{
    public class PlayerProvider
    {
        // Provides a random player pool to be used for testing
        // The positions in the player pool are determined by the positions array
        // The number at each position is random but will be between
        // minAtPosition and 10 + minAtPosition
        // supports a player pool with up to 8 different positions
        // Also randomly preselects players as "chosen" for a lineup, so
        // that the optimization algorithm can work with some players already having
        // been chosen
        public static IList<Player> GetPlayersRandom(Position[] positions, int minAtPosition, int numGames, int numTeamsPerGame)
        {
            var random = new Random();
            var players = new List<Player>();
            var id = 1;
            foreach (var position in positions)
            {
                var numPlayers = random.Next(minAtPosition, minAtPosition + 10);
                for (var i = 0; i < numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30) + ((decimal)random.Next(0, 99)) / 100;
                    var playerPositions = new HashSet<string> { position.Name };
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                    {
                        var randomPositionIndex = random.Next(0, positions.Length);
                        var position2 = positions[randomPositionIndex];
                        if (!playerPositions.Contains(position2.Name))
                            playerPositions.Add(position2.Name);
                    }
                    // randomly pick a game Id
                    var gameId = random.Next(1, numGames + 1) * 10;
                    var teamId = (gameId * 10) + random.Next(1, numTeamsPerGame + 1);
                    players.Add(new Player(positions.Select(pos => pos.Name).ToArray())
                    {
                        Id = id++,
                        Name = name,
                        ProjectedPoints = projectedPoints,
                        Positions = playerPositions,
                        Salary = salary,
                        GameId = gameId,
                        TeamId = teamId
                    });
                }
            }

            // randomly prefill n players
            var n = random.Next(1, positions.Length + 1);
            var chosenPlayers = new List<Player>();
            for (var i = 0; i < n; i++)
            {
                var position = positions[i];
                var eligiblePlayers = players.Where(p => p.Positions.Contains(position.Name)).ToList();
                var eligiblePlayerCount = eligiblePlayers.Count;
                var randEligible = random.Next(0, eligiblePlayerCount);
                var player = eligiblePlayers[randEligible];
                if (chosenPlayers.Contains(player)) continue; // don't re-choose player for another position
                chosenPlayers.Add(player);
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
