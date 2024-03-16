namespace RussianVotingTools.UI.Analyzer.ViewModels
{
    public sealed class ElectionObservationViewModel
        : BaseDatabaseObjectViewModel
    {
		private string _name;
		public ElectionObservationViewModel(long id)
            : base(id)
        {
        }

		public string Name
		{
			get { return _name; }
			set { OnPropertyChanged(ref _name, value, new(nameof(Name))); }
		}
	}
}
