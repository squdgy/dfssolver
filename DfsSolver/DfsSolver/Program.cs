// 

using System.Linq;

namespace DfsSolver
{
    internal class Program
    {
        private static void Main()
        {
            LineupOptimizer.Solve(PlayerProvider.GetPlayersRandom().ToList());
        }
    }
}
