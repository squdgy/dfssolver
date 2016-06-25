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
        public double ChosenAtPosition0 { get; set; }
        public double ChosenAtPosition1 { get; set; }
        public double ChosenAtPosition2 { get; set; }
        public double ChosenAtPosition3 { get; set; }
        public double ChosenAtPosition4 { get; set; }
        public double ChosenAtPosition5 { get; set; }
        public double ChosenAtPosition6 { get; set; }
        public double ChosenAtPosition7 { get; set; }
        public double ChosenAtPosition8 { get; set; }

        // used for solver Parameter binding
        public double EligibleAtPosition0 => Positions.Contains("P") ? 1 : 0;
	    public double EligibleAtPosition1 => Positions.Contains("C") ? 1 : 0;
        public double EligibleAtPosition2 => Positions.Contains("1B") ? 1 : 0;
        public double EligibleAtPosition3 => Positions.Contains("2B") ? 1 : 0;
        public double EligibleAtPosition4 => Positions.Contains("3B") ? 1 : 0;
        public double EligibleAtPosition5 => Positions.Contains("SS") ? 1 : 0;
        public double EligibleAtPosition6 => Positions.Contains("LF") ? 1 : 0;
        public double EligibleAtPosition7 => Positions.Contains("CF") ? 1 : 0;
        public double EligibleAtPosition8 => Positions.Contains("RF") ? 1 : 0;

        public bool Chosen => ChosenAtPosition0 == 1 && EligibleAtPosition0 == 1 ||
	                          ChosenAtPosition1 == 1 && EligibleAtPosition1 == 1 || 
	                          ChosenAtPosition2 == 1 && EligibleAtPosition2 == 1 || 
	                          ChosenAtPosition3 == 1 && EligibleAtPosition3 == 1 || 
	                          ChosenAtPosition4 == 1 && EligibleAtPosition4 == 1 || 
	                          ChosenAtPosition5 == 1 && EligibleAtPosition5 == 1 || 
	                          ChosenAtPosition6 == 1 && EligibleAtPosition6 == 1 || 
	                          ChosenAtPosition7 == 1 && EligibleAtPosition7 == 1 || 
	                          ChosenAtPosition8 == 1 && EligibleAtPosition8 == 1;

	    public override string ToString()
	    {
	        var positions = "";
	        foreach (var pos in Positions)
	            positions += pos + ",";
	        var selectedPos = "";
	        if (ChosenAtPosition0 == 1)
	            selectedPos += "P";
	        if (ChosenAtPosition1 == 1)
	            selectedPos += "C";
	        if (ChosenAtPosition2 == 1)
	            selectedPos += "1B";
	        if (ChosenAtPosition3 == 1)
	            selectedPos += "2B";
	        if (ChosenAtPosition4 == 1)
	            selectedPos += "3B";
	        if (ChosenAtPosition5 == 1)
	            selectedPos += "SS";
	        if (ChosenAtPosition6 == 1)
	            selectedPos += "LF";
	        if (ChosenAtPosition7 == 1)
	            selectedPos += "CF";
	        if (ChosenAtPosition8 == 1)
	            selectedPos += "RF";
            return $"{selectedPos} {Salary} {ProjectedPoints} {Name} {positions}";
		}
	}
}
