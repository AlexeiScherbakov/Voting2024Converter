using System.Collections.ObjectModel;

namespace RussianVotingTools.UI.Analyzer.ViewModels
{
    public sealed class ElectionTimelineViewModel
        : BaseDatabaseObjectViewModel
    {
		private string _name;
		private DateTimeOffset _plannedStartTime;
		private DateTimeOffset _plannedEndTime;
		private DateTimeOffset? _startTime;
		private DateTimeOffset? _endTime;

		private ObservableCollection<ElectionObservationViewModel> _electionObservations = new();

		public ElectionTimelineViewModel(long id)
            : base(id)
        {
        }

		public string Name
		{
			get { return _name; }
			set { OnPropertyChanged(ref _name, value, new(nameof(Name))); }
		}

		public DateTimeOffset PlannedStartTime
		{
			get { return _plannedStartTime; }
			set { OnPropertyChanged(ref _plannedStartTime, value, new(nameof(PlannedStartTime))); }
		}

		public DateTimeOffset PlannedEndTime
		{
			get { return _plannedEndTime; }
			set { OnPropertyChanged(ref _plannedEndTime, value, new(nameof(PlannedEndTime))); }
		}

		public DateTimeOffset? StartTime
		{
			get { return _startTime; }
			set { OnPropertyChanged(ref _startTime, value, new(nameof(StartTime))); }
		}

		public DateTimeOffset? EndTime
		{
			get { return _endTime; }
			set { OnPropertyChanged(ref _endTime, value, new(nameof(EndTime))); }
		}

		public ObservableCollection<ElectionObservationViewModel> ElectionObservations
		{
			get { return _electionObservations; }
		}
	}
}
