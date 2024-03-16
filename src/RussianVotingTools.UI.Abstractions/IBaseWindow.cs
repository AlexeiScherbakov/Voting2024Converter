namespace RussianVotingTools.UI.Abstractions
{
	/// <summary>
	/// Базовое окно, кроме главного
	/// </summary>
	public interface IBaseWindow
		: ICanBeParentWindow
	{
		IWindowChrome Chrome { get; }
	}


	public interface IViewModelWindow<T>
		: IBaseWindow
	{
		T ViewModel { get; }
	}


	public interface IEditorWindow<T>
		: IBaseWindow
	{
		T EditableObject { get; set; }
	}
}
