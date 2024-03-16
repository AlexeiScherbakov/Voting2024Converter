using System.ComponentModel.DataAnnotations;

namespace RussianVotingTools.UI.Abstractions
{
	public interface IWindowFactory
	{
		/// <summary>
		/// Создает окно
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		ValueTask<T> CreateWindowAsync<T>() where T : IBaseWindow;


		/// <summary>
		/// Создает окно ввода текста
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="defaultText"></param>
		/// <param name="dataType"></param>
		/// <returns></returns>
		ValueTask<string> InputText(string caption, string defaultText, DataTypeAttribute dataType);
	}
}
