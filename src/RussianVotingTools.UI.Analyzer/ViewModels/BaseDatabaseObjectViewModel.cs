namespace RussianVotingTools.UI.Analyzer.ViewModels
{
	public abstract class BaseDatabaseObjectViewModel
        : NotifyPropertyChangedBase
    {
        private readonly long _id;

        protected BaseDatabaseObjectViewModel(long id)
        {
            _id = id;
        }

        public long Id
        {
            get { return _id; }
        }
    }
}
