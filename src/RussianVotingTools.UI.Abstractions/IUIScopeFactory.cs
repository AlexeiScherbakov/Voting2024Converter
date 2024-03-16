namespace RussianVotingTools.UI.Abstractions
{
	public interface IUIScopeFactory
	{
		/// <summary>
		/// Создает Scope для создания под-окон интерфейса и взаимодействий
		/// </summary>
		/// <returns></returns>
		IUIScope CreateUIScope();
	}
}
