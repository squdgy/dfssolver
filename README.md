# dfssolver
Example of how to use Microsoft Solver Foundation to build an optimal DFS lineup

Assumes lineup makeup of DraftKings MLB 2016, but should work with other lineup configurations easily
Uses randomly generated player data
Uses LPSolve as the solver, available here: https://sourceforge.net/projects/lpsolve/
Supports multi-position eligibility for players (up to 2 positions)

Does Not:

Have constraints for stacking rules or other players/team, players/game rules