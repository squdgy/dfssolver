using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DfsSolver
{
    public class LineupOptimizer
    {
        public static void Solve(IEnumerable<Player> playerData)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();
            var players = new Set(Domain.Any, "players");

            // Salary for each player
            var salary = new Parameter(Domain.IntegerNonnegative, "salary", players);
            salary.SetBinding(playerData, "Salary", "Id");
            model.AddParameter(salary);

            // Projected fantasy points for each player.
            var projectedPoints = new Parameter(Domain.IntegerNonnegative, "projectedPoints", players);
            projectedPoints.SetBinding(playerData, "ProjectedPoints", "Id");
            model.AddParameter(projectedPoints);

            var pit = new Parameter(Domain.Integer, "pitcherVal", players);
            pit.SetBinding(playerData, "PitcherVal", "Id");
            model.AddParameter(pit);

            var cat = new Parameter(Domain.Integer, "catcherVal", players);
            cat.SetBinding(playerData, "CatcherVal", "Id");
            model.AddParameter(cat);

            // Constants
            int rosterSize = 2;
            int numPitchers = 1;
            int numCatchers = 1;
            int salaryCap = 6000;

            //var choose = new Decision(Domain.IntegerRange(0, 1), "choose", players);
            //choose.SetBinding(theData, "InLineup", "Id");
            //model.AddDecision(choose);

            var pitchers = playerData.Where(p => p.Position == "P");
            var catchers = playerData.Where(p => p.Position == "C");
            var chooseP = new Decision(Domain.IntegerRange(0, 1), "chooseP", players);
            chooseP.SetBinding(playerData, "ChosenPitcher", "Id");
            model.AddDecision(chooseP);
            var chooseC = new Decision(Domain.IntegerRange(0, 1), "chooseC", players);
            chooseC.SetBinding(playerData, "ChosenCatcher", "Id");
            model.AddDecision(chooseC);

            // Reserve the right number of players.
            model.AddConstraint("numPlayers", Model.Sum(Model.ForEach(players, i => chooseP[i] + chooseC[i])) == rosterSize);
            // Reserve the right number of pitchers.
            model.AddConstraint("pitchers", (Model.Sum(Model.ForEach(players, i => chooseP[i] * pit[i])) == numPitchers));
            // Reserve the right number of catchers.
            model.AddConstraint("catchers", (Model.Sum(Model.ForEach(players, i => chooseC[i] * cat[i])) == numCatchers));

            // within the salary cap
            var pTerm = Model.Sum(Model.ForEach(players, i => (chooseP[i] + chooseC[i]) * salary[i]));
            //var cTerm = Model.Sum(Model.ForEach(players, i => (chooseC[i] * salary[i])));
            model.AddConstraint("maxSalary", pTerm <= salaryCap);

            // maximize for projectedPoints
            var psTerm = Model.Sum(Model.ForEach(players, i => chooseP[i] * projectedPoints[i]));
            var csTerm = Model.Sum(Model.ForEach(players, i => chooseC[i] * projectedPoints[i]));
            model.AddGoal("maxPoints", GoalKind.Maximize, Model.Sum(psTerm, csTerm));

            var solution = context.Solve();
            if (solution.Quality == SolverQuality.InfeasibleOrUnbounded)
            {
                Log("No Solution!");
                return;
            }
            Log(solution.GetReport().ToString());
            context.PropagateDecisions();
            var selected = from d in playerData
                           where d.IsChosenCatcher || d.IsChosenPitcher
                           select d;

            Log(string.Format("Player Pool: {0} total: {1} pitchers, {2} catchers",
                playerData.Count(), playerData.Count(p => p.Position == "P"), playerData.Count(p => p.Position == "C")));
            var totalProjectedPoints = 0;
            var totalSalary = 0;
            foreach (var s in selected)
            {
                totalProjectedPoints += s.ProjectedPoints;
                totalSalary += s.Salary;
                Log(s.ToString());
            }
            Log(string.Format("Projected Points: {0}, Used Salary: {1}", totalProjectedPoints, totalSalary));
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            Debug.WriteLine(text);
        }
    }
}
