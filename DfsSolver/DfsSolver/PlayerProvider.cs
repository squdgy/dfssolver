﻿using System;
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
                var numPlayers = random.Next(15,16);
                for (var i = 0; i< numPlayers; i++)
                {
                    var name = $"Joe_{id} Player";
                    var salary = random.Next(3000, 8000);
                    var projectedPoints = random.Next(0, 30);
                    var playerPositions = new List<Position> {position1};
                    var hasSecondPosition = random.Next(0, 2) == 1;
                    if (hasSecondPosition)
                    {
                        var position2 = 
                            positions[random.Next(positions.Min(rs => rs.Id) - 1, positions.Max(rs => rs.Id))];
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
            return players;
        }
    }
}
