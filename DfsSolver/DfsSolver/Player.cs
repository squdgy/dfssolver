// Describes a player in a way that Solver can bind with it

using System.Collections.Generic;

namespace DfsSolver
{
	public class Player
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
	    public decimal ProjectedPoints { get; set; }
		public int Salary { get; internal set; }

        public List<Position> Positions { get; internal set; }
        public int PositionId1 => Positions.Count >= 1 ? Positions[0].Id : 0;
        public int PositionId2 => Positions.Count >= 2 ? Positions[1].Id : 0;

        // used for solver Decision binding
        public int ProjectedPointsAsInt => (int)(ProjectedPoints * 100);
        public double DraftPositionId { get; set; }
	    public bool IsDrafted => DraftPositionId > 0;

	    public override string ToString()
	    {
	        var positions = "";
	        var draftPosition = "";
	        foreach (var pos in Positions)
	        {
	            positions += pos + " ";
	            if (DraftPositionId == pos.Id)
	                draftPosition = pos.ToString();
	        }
            return $"{Salary} {draftPosition} {ProjectedPoints} {Name} {positions}";
		}
	}
}
