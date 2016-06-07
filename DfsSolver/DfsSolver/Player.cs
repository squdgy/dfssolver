// Describes a player in a way that Solver can bind with it

using System.Collections.Generic;

namespace DfsSolver
{
	public class Player
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public HashSet<string> Positions { get; internal set; }
		public int ProjectedPoints { get; internal set; }
		public int Salary { get; internal set; }

        // used for solver Decision binding
        public double Chosen { get; set; }

        // used for solver Parameter binding
        public double IsPitcherVal => Positions.Contains("P") ? 1 : 0;
	    public double IsCatcherVal => Positions.Contains("C") ? 1 : 0;
	    public double Is1BVal => Positions.Contains("1B") ? 1 : 0;
	    public double Is2BVal => Positions.Contains("2B") ? 1 : 0;
	    public double Is3BVal => Positions.Contains("3B") ? 1 : 0;
	    public double IsSSVal => Positions.Contains("SS") ? 1 : 0;
	    public double IsLFVal => Positions.Contains("LF") ? 1 : 0;
	    public double IsCFVal => Positions.Contains("CF") ? 1 : 0;
	    public double IsRFVal => Positions.Contains("RF") ? 1 : 0;

	    public bool IsChosen => Chosen == 1;

	    public override string ToString()
	    {
	        var positions = "";
	        foreach (var pos in Positions)
	            positions += pos + ",";
	        var selectedPos = "";
            return $"{selectedPos} {Salary} {ProjectedPoints} {Name} {positions}";
		}
	}
}
