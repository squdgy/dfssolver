# dfssolver

Example of how to use Microsoft Solver Foundation to build an optimal DFS lineup
=================================================================================

* Assumes lineup makeup of DraftKings MLB 2016, but should work with other lineup configurations easily
* Uses randomly generated player data
* Uses LPSolve as the solver, included, source available here: https://sourceforge.net/projects/lpsolve/
* Supports multi-position eligibility for players (up to 2 positions)
* Does not have constraints for stacking rules or other rules, such as players/team, players/game rules

What to do to actually use this
=================================================================================
* Replace the randomly generated player pool with an actual player pool
* Make sure the actual player pool includes projected points for players 
* Change the definition of the lineup definition as needed (see Program.cs)
