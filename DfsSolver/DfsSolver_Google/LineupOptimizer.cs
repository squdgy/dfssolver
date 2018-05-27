using DfsBase;
using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver_Google
{
    public class LineupOptimizer
    {
        private const int TIME_LIMIT_IN_MS = 5000;

        /// <summary>
        /// Generate an optimal lineup which maximizes for projected points
        /// </summary>
        /// <param name="playerPool">all players, including already selected ones</param>
        /// <param name="lineupSlots">all lineup positions with the count of how many to draft</param>
        /// <param name="salaryCap">salary cap for the contest</param>
        /// <returns></returns>
        public static LineupSolution Solve(IList<Player> playerPool, IList<LineupSlot> lineupSlots, ContestRules rules)
        {
            // deal with pre filled slots
            var validLineupCount = lineupSlots.Sum(ls => ls.Count);
            var prefilled = playerPool.Where(p => p.Chosen).ToList();
            var availablePlayers = playerPool.Except(prefilled).ToList();
            var unfilledCap = rules.SalaryCap - prefilled.Sum(p => p.Salary);
            var unfilledSlots = new List<LineupSlot>();
            var prefilledGames = prefilled.Select(p => p.GameId).Distinct();
            var numGamesNeeded = Math.Max(rules.MinGames - prefilledGames.Count(), 0);
            var slotIndex = 0;
            foreach (var slot in lineupSlots)
            {
                var prefilledCount = prefilled.Count(p => p.ChosenPosition == slot.Name);
                var newCountForSlot = slot.Count - prefilledCount;
                unfilledSlots.Add(new LineupSlot
                {
                    Name = slot.Name,
                    Count = newCountForSlot,
                    SlotIndex = slotIndex
                });
                slotIndex++;
            }

            // trim the draft pool by removing those whose draft positions don't need to be filled
            var unfilledPosNames = unfilledSlots.Where(ls => ls.Count > 0).Select(ls => ls.Name).ToList();
            availablePlayers = availablePlayers.Where(
                    p => unfilledPosNames.Intersect(p.Positions).Any()).ToList();

            // do a further trim, such that for each set of eligibilities there is at most 1 player
            // who meets those eligibilities and has a projected point value of 0
            // that player should be the lowest salaried player
            var groupings = availablePlayers.Where(a => a.ProjectedPoints == 0).GroupBy(a => a.PositionsText);

            // Using a constraint solver, so far uses salary, projected points and salary cap
            var solver = new Solver("lineup optimizer");

            // ---- Define Parameters----
            // global:
            //      1. salary cap 
            // for each player: 
            //      2. salary
            //      3. gameId
            //      4. teamId
            //      5. projected points
            //      for each unfilled slot: 
            //          6. whether or not that player is eligible to play in that slot
            var cap = unfilledCap;
            var salaries = availablePlayers.Select(ap => ap.Salary).ToArray();
            var projectedPoints = availablePlayers.Select(ap => (int)(ap.ProjectedPoints * 1000)).ToArray();
            var availableSlots = unfilledSlots.Where(us => us.Count > 0).ToList();
            int[,] eligibilities = new int[availableSlots.Count, availablePlayers.Count];
            for (var i=0; i< availableSlots.Count; i++)
            {
                for (var j=0; j< availablePlayers.Count; j++)
                {
                    eligibilities[i, j] = availablePlayers[j].Positions.Contains(availableSlots[i].Name) ? 1 : 0;
                }
            }

            // ---- Define Decision Variables ----
            // for each player and slot:
            //      1. boolean value indicating if the player will be included
            var isChosen = solver.MakeBoolVarMatrix(availableSlots.Count, availablePlayers.Count, "isChosen");
            var isChosen_flat = isChosen.Flatten();

            // ---- Define Constraints ----

            // Reserve the right number of each position
            for (int i = 0; i < availableSlots.Count; i++)
            {
                solver.Add((from j in Enumerable.Range(0, availablePlayers.Count)
                            select isChosen[i, j]).ToArray().Sum() == availableSlots[i].Count);
            }

            // players can only be in positions that they are eligible for
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                for (int j = 0; j < availableSlots.Count; j++)
                {
                    solver.Add(isChosen[j, i] <= eligibilities[j, i]);
                }
            }

            // players can only be in the player pool at 1 position
            for (int i = 0; i < availablePlayers.Count; i++)
            {
                solver.Add((from j in Enumerable.Range(0, availableSlots.Count)
                            select isChosen[j, i]).ToArray().Sum() <= 1);
            }

            // can't go over salary
            solver.Add((from i in Enumerable.Range(0, availableSlots.Count)
                       from j in Enumerable.Range(0, availablePlayers.Count)
                       select (salaries[j] * isChosen[i, j])).ToArray().Sum() <= cap);

            // There have to be at least so many games represented,
            // If there's only 1 game needed or games constraint has been satisfied by prefilled
            // players, then skip constraint
            if (rules.MinGames > 1 && numGamesNeeded > 0)
            {
                // TODO: use global cardinality constraint?
                var playerGames = availablePlayers.Select(ap => ap.GameId).ToArray();
                var gameIds = playerGames.Distinct().ToList();
                var gamesUsed = solver.MakeBoolVarArray(gameIds.Count());

                for (int i = 0; i < gameIds.Count; i++)
                {
                    // sum the # of prefilled instances of this game with the 
                    // decision instances of this game and set gameUsed to true if that sum is
                    // greater than 0
                    solver.Add(gamesUsed[i] ==
                                (prefilledGames.Contains(gameIds[i]) ? 1 : 0) +
                                (from j in Enumerable.Range(0, availableSlots.Count)
                                from k in Enumerable.Range(0, availablePlayers.Count)
                                select (playerGames[k] * isChosen[j, k] == gameIds[i])).ToArray().Sum() > 0);
                }

                solver.Add((from i in Enumerable.Range(0, gameIds.Count)
                            select gamesUsed[i]).ToArray().Sum() >= numGamesNeeded);
            }

            // TODO: Create More Constraints
            // - max per team
            // - min teams

            // ---- Define the Goal ----
            // maximize for projectedPoints
            IntVar total_projected_points = (from i in Enumerable.Range(0, availableSlots.Count)
                                 from j in Enumerable.Range(0, availablePlayers.Count)
                                 select (projectedPoints[j] * isChosen[i, j])).ToArray().Sum().Var();
            var objective = solver.MakeMaximize(total_projected_points, 1);

            // create the main decision builder phase of the solver
            var db = solver.MakePhase(isChosen_flat, Solver.INT_VAR_DEFAULT, Solver.ASSIGN_MAX_VALUE);

            // create a collector - this will collect all solutions and make them available at the end
            var collector = solver.MakeAllSolutionCollector();
            collector.Add(isChosen_flat);
            collector.AddObjective(total_projected_points);

            // set a time limit; will return the best solution within the time limit
            var solverTimeLimit = solver.MakeTimeLimit(TIME_LIMIT_IN_MS);

            // create a search logger - useful for debugging (only use then)
            // reports after each solution and after each 1000 branches
            //var searchLogger = solver.MakeSearchLog(1000, objective);
            //bool solutionFound = solver.Solve(db, searchLogger, objective, solverTimeLimit, collector);

            var startingWallTime = solver.WallTime();
            bool solutionFound = solver.Solve(db, objective, solverTimeLimit, collector);
            var elapsedWallTime = solver.WallTime() - startingWallTime;
            if (!solutionFound)
            {
                Log("No solution found");
                return null;
            }

            var numSolutions = collector.SolutionCount();
            var selectedPlayers = new List<Player>();
            for (var soli = 0; soli < collector.SolutionCount(); soli++)
            {
                // clear out solution from last time through
                foreach (var ap in availablePlayers)
                {
                    ap.ClearChosen();
                }
                for (int i = 0; i < availableSlots.Count; i++)
                {
                    for (int j = 0; j < availablePlayers.Count; j++)
                    {
                        var val = collector.Value(soli, isChosen[i, j]);
                        if (val == 1)//(isChosen[i, j].Value() == 1)
                        {
                            var posindex = availableSlots[i].SlotIndex;
                            if (posindex == 0) availablePlayers[j].ChosenAtPosition0 = 1;
                            if (posindex == 1) availablePlayers[j].ChosenAtPosition1 = 1;
                            if (posindex == 2) availablePlayers[j].ChosenAtPosition2 = 1;
                            if (posindex == 3) availablePlayers[j].ChosenAtPosition3 = 1;
                            if (posindex == 4) availablePlayers[j].ChosenAtPosition4 = 1;
                            if (posindex == 5) availablePlayers[j].ChosenAtPosition5 = 1;
                            if (posindex == 6) availablePlayers[j].ChosenAtPosition6 = 1;
                            if (posindex == 7) availablePlayers[j].ChosenAtPosition7 = 1;
                            if (posindex == 8) availablePlayers[j].ChosenAtPosition8 = 1;
                            //Log($"{availablePlayers[j].Name} chosen");
                        }
                    }
                }
                if (playerPool.Count(p => p.Chosen) != validLineupCount)
                {
                    Log("No Solution! Count of players doesn't match.");
                    //ReportSolution(playerPool, lineupSlots, prefilled, availablePlayers);
                }
                else
                {
                    ReportSolution(playerPool, lineupSlots, prefilled, availablePlayers);
                }
            }

            return new LineupSolution
            {
                Lineup = playerPool.Where(p => p.Chosen).ToList(),
                IsOptimal = elapsedWallTime <= TIME_LIMIT_IN_MS
            };
        }

        private static void ReportSolution(ICollection<Player> playerPool,
            IList<LineupSlot> lineupSlots,
            ICollection<Player> prefilled, ICollection<Player> available)
        {
            Log("====================================================");
            Log($"Pre-filled players: {prefilled.Count}");
            Log("=========================================");
            foreach (var lineupSlot in lineupSlots)
            {
                var atSlot = prefilled.Where(s => s.ChosenPosition == lineupSlot.Name);
                foreach (var s in atSlot)
                {
                    Log(s.ToString());
                }
            }
            Log("=========================================");

            var selected = playerPool.Where(p => p.Chosen).ToList();
            Log($"Player Pool Size: {playerPool.Count}, After prefill exclusions: {available.Count}");
            Log("====================================================");
            Log("Lineup");
            Log("====================================================");
            var totalProjectedPoints = 0m;
            var totalSalary = 0;
            foreach (var lineupSlot in lineupSlots)
            {
                var atSlot = selected.Where(s => s.ChosenPosition == lineupSlot.Name);
                foreach (var s in atSlot)
                {
                    totalProjectedPoints += s.ProjectedPoints;
                    totalSalary += s.Salary;
                    Log(s.ToString());
                }
            }
            Log("=========================================");
            Log($"Projected Points: {totalProjectedPoints}, Used Salary: {totalSalary}");
            Log("====================================================");
        }
        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
