// Describes a player in a way that Solver can bind with it

using System.Collections.Generic;
using System.Linq;

namespace DfsBase
{
	public class Player
	{
	    private readonly string[] _allPositions;

        public Player(string[] allPositions)
	    {
	        _allPositions = allPositions;
	    }

        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public HashSet<string> Positions { get; internal set; }
        public string PositionsText => Positions.Aggregate("", (current, pos) => current + pos + ",");
        public decimal ProjectedPoints { get; internal set; }
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

	    public string ChosenPosition
	    {
	        get
	        {
                var selectedPos = "";
                if (ChosenAtPosition0 == 1)
                    selectedPos += _allPositions[0];
                if (ChosenAtPosition1 == 1)
                    selectedPos += _allPositions[1];
                if (ChosenAtPosition2 == 1)
                    selectedPos += _allPositions[2];
                if (ChosenAtPosition3 == 1)
                    selectedPos += _allPositions[3];
                if (ChosenAtPosition4 == 1)
                    selectedPos += _allPositions[4];
                if (ChosenAtPosition5 == 1)
                    selectedPos += _allPositions[5];
                if (ChosenAtPosition6 == 1)
                    selectedPos += _allPositions[6];
                if (ChosenAtPosition7 == 1)
                    selectedPos += _allPositions[7];
                if (ChosenAtPosition8 == 1)
                    selectedPos += _allPositions[8];
	            return selectedPos;
	        }
        }

        // used for solver Parameter binding
        public int ProjectedPointsAsInt => (int)(ProjectedPoints * 100);
        public double EligibleAtPosition0 => Positions.Contains(_allPositions[0]) ? 1 : 0;
	    public double EligibleAtPosition1 => Positions.Contains(_allPositions[1]) ? 1 : 0;
        public double EligibleAtPosition2 => Positions.Contains(_allPositions[2]) ? 1 : 0;
        public double EligibleAtPosition3 => Positions.Contains(_allPositions[3]) ? 1 : 0;
        public double EligibleAtPosition4 => Positions.Contains(_allPositions[4]) ? 1 : 0;
        public double EligibleAtPosition5 => Positions.Contains(_allPositions[5]) ? 1 : 0;
        public double EligibleAtPosition6 => Positions.Contains(_allPositions[6]) ? 1 : 0;
        public double EligibleAtPosition7 => Positions.Contains(_allPositions[7]) ? 1 : 0;
        public double EligibleAtPosition8 => Positions.Contains(_allPositions[8]) ? 1 : 0;

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
	        return $"{ChosenPosition} {Salary} {ProjectedPoints} {Name} {PositionsText}";
		}

        public void ClearChosen()
        {
            ChosenAtPosition0 = 0;
            ChosenAtPosition1 = 0;
            ChosenAtPosition2 = 0;
            ChosenAtPosition3 = 0;
            ChosenAtPosition4 = 0;
            ChosenAtPosition5 = 0;
            ChosenAtPosition6 = 0;
            ChosenAtPosition7 = 0;
            ChosenAtPosition8 = 0;
        }
    }
}
