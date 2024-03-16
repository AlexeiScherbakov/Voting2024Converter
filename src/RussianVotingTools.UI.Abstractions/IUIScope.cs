namespace RussianVotingTools.UI.Abstractions
{
	public interface IUIScope
		: IDisposable
	{
		IWindowFactory WindowFactory { get; }
	}
}
