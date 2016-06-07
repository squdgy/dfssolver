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
        public double ChosenPitcher { get; set; }
        public double ChosenCatcher { get; set; }
        public double Chosen1B { get; set; }
        public double Chosen2B { get; set; }
        public double Chosen3B { get; set; }
        public double ChosenSS { get; set; }
        public double ChosenLF { get; set; }
        public double ChosenCF { get; set; }
        public double ChosenRF { get; set; }

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

	    public bool Chosen => ChosenPitcher == 1 && IsPitcherVal == 1 ||
	                          ChosenCatcher == 1 && IsCatcherVal == 1 || 
	                          Chosen1B == 1 && Is1BVal == 1 || 
	                          Chosen2B == 1 && Is2BVal == 1 || 
	                          Chosen3B == 1 && Is3BVal == 1 || 
	                          ChosenSS == 1 && IsSSVal == 1 || 
	                          ChosenLF == 1 && IsLFVal == 1 || 
	                          ChosenCF == 1 && IsCFVal == 1 || 
	                          ChosenRF == 1 && IsRFVal == 1;

	    public override string ToString()
	    {
	        var positions = "";
	        foreach (var pos in Positions)
	            positions += pos + ",";
	        var selectedPos = "";
	        if (ChosenPitcher == 1)
	            selectedPos += "P";
            if (ChosenCatcher == 1)
                selectedPos += "C";
            if (Chosen1B == 1)
                selectedPos += "1B";
            if (Chosen2B == 1)
                selectedPos += "2B";
            if (Chosen3B == 1)
                selectedPos += "3B";
            if (ChosenSS == 1)
                selectedPos += "SS";
            if (ChosenLF == 1)
                selectedPos += "LF";
            if (ChosenCF == 1)
                selectedPos += "CF";
            if (ChosenRF == 1)
                selectedPos += "RF";
            return $"{selectedPos} {Salary} {ProjectedPoints} {Name} {positions}";
		}
	}
}
