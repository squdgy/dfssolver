// Describes a player in a way that Solver can bind with it
namespace DfsSolver
{
	public class Player
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public string Position { get; internal set; }
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
        public double IsPitcherVal => Position == "P" ? 1 : 0;
	    public double IsCatcherVal => Position == "C" ? 1 : 0;
	    public double Is1BVal => Position == "1B" ? 1 : 0;
	    public double Is2BVal => Position == "2B" ? 1 : 0;
	    public double Is3BVal => Position == "3B" ? 1 : 0;
	    public double IsSSVal => Position == "SS" ? 1 : 0;
	    public double IsLFVal => Position == "LF" ? 1 : 0;
	    public double IsCFVal => Position == "CF" ? 1 : 0;
	    public double IsRFVal => Position == "RF" ? 1 : 0;

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
			return $"{Position} {Salary} {ProjectedPoints} {Name}";
		}
	}
}
