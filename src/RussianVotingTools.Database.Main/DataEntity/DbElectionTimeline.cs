using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianVotingTools.Database.Main.DataEntity
{
	public class DbElectionTimeline
		: DbIdObject
	{
		private string _name;
		private DateTime _plannedStartTime;
		private DateTime _plannedEndTime;
		private DateTime? _startTime;
		private DateTime? _endTime;
		private ISet<DbElectionObservation> _electionObservations = new HashSet<DbElectionObservation>();

		public DbElectionTimeline()
		{

		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual DateTime PlannedStartTime
		{
			get { return _plannedStartTime; }
			set { _plannedStartTime = value; }
		}

		public virtual DateTime PlannedEndTime
		{
			get { return _plannedEndTime; }
			set { _plannedEndTime = value; }
		}

		public virtual DateTime? StartTime
		{
			get { return _startTime; }
			set { _startTime = value; }
		}

		public virtual DateTime? EndTime
		{
			get { return _endTime; }
			set { _endTime = value; }
		}

		public virtual ISet<DbElectionObservation> ElectionObservations
		{
			get { return _electionObservations; }
			protected set { _electionObservations = value; }
		}
	}
}
