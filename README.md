# dfssolver

Examples of how to use Microsoft Solver Foundation and Google Or-Tools to build an optimal DFS lineup
=====================================================================================================
There are 2 projects, which each do the exact same thing, so you can compare/contrast.
Microsoft Solver Foundation is no longer being worked on, so OrTools is the preferable option.

* Assumes lineup makeup of DraftKings MLB 2016, but should work with other lineup configurations easily
* Uses randomly generated player data
* Uses LPSolve as the solver, included, source available here: https://sourceforge.net/projects/lpsolve/
* Supports multi-position eligibility for players (tested with up to 2 positions)
* Does not have constraints for stacking rules or other rules, such as players/team, players/game rules
* Does not support flex/util positions

What to do to actually use this
=================================================================================
* Replace the randomly generated player pool with an actual player pool
* Make sure the actual player pool includes projected points for players 
* Change the definition of the lineup definition as needed (see Program.cs)
* To pre-select some players in the pool, you can either:
+ remove the lineup slot, remove the player from the player pool, and subtract the player's salary from the salaryCap OR
+ mark players as already selected by setting ChosenAtPosition<n> = 1, where n is an 0-based index into the lineup slots list
> Example: if the lineups were "P":2, "C":1, "1B":2, then 
> to preselect a pitcher -> player.ChosenAtPosition0 = 1,
> to preselect a catcher -> player.ChosenAtPosition1 = 1,
> to preselect a firstbasemen -> player.ChosenAtPosition2 = 1
> etc.