// Describes a player in a way that Solver can bind with it
namespace DfsSolver
{
	public class Player
	{
		//private double _inLineupValue;
		//public bool IsInLineup
		//{
		//	get { return _inLineupValue == 1; }
		//	set { _inLineupValue = value ? 1 : 0; }
		//}
		//public double InLineup
		//{
		//	get { return _inLineupValue; }
		//	set { _inLineupValue = value; }
		//}

		private double _isChosenPitcher;
		public bool IsChosenPitcher
		{
			get { return _isChosenPitcher == 1; }
			set { _isChosenPitcher = value ? 1 : 0; }
		}
		public double ChosenPitcher
		{
			get { return _isChosenPitcher; }
			set { _isChosenPitcher = value; }
		}

		private double _isChosenCatcher;
		public bool IsChosenCatcher
		{
			get { return _isChosenCatcher == 1; }
			set { _isChosenCatcher = value ? 1 : 0; }
		}
		public double ChosenCatcher
		{
			get { return _isChosenCatcher; }
			set { _isChosenCatcher = value; }
		}

		public double PitcherVal
		{
			get { return Position == "P" ? 1 : 0; }
		}
		public double CatcherVal
		{
			get { return Position == "C" ? 1 : 0; }
		}

		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public string Position { get; internal set; }
		public int ProjectedPoints { get; internal set; }
		public int Salary { get; internal set; }

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3}", Position, Salary, ProjectedPoints, Name);
		}

	}
}
