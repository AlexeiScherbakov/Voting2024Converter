namespace RussianVotingTools.UI.Abstractions
{
	public abstract class SiteCommand
	{
		public abstract Task ExecuteAsync(ICommandSite site);
	}
}
