namespace RussianVotingTools.Database.Main.Abstractions
{
	public sealed class ElectionTimelineData
	{
		private string _name;
		private DateTimeOffset _plannedStartTime;
		private DateTimeOffset _plannedEndTime;
		private DateTimeOffset? _startTime;
		private DateTimeOffset? _endTime;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public DateTimeOffset PlannedStartTime
		{
			get { return _plannedStartTime; }
			set { _plannedStartTime = value; }
		}

		public DateTimeOffset PlannedEndTime
		{
			get { return _plannedEndTime; }
			set { _plannedEndTime = value; }
		}

		public DateTimeOffset? StartTime
		{
			get { return _startTime; }
			set { _startTime = value; }
		}

		public DateTimeOffset? EndTime
		{
			get { return _endTime; }
			set { _endTime = value; }
		}
	}
}
