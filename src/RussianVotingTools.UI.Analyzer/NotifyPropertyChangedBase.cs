using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RussianVotingTools.UI.Analyzer
{
	public abstract class NotifyPropertyChangedBase
		: INotifyPropertyChanged
	{
		private bool _notificationDisabled;

		protected void DisableNotifications()
		{
			_notificationDisabled = true;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			// проверка включены ли нотификации
			if (_notificationDisabled)
			{
				return;
			}

			// нотификация
			var evnt = PropertyChanged;
			if (evnt != null)
			{
				evnt(this, args);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void OnPropertyChanged<TProperty>(ref TProperty field, TProperty value, PropertyChangedEventArgs args)
		{
			// установка поля
			if (EqualityComparer<TProperty>.Default.Equals(field, value))
			{
				return;
			}
			field = value;

			// проверка включены ли нотификации
			if (_notificationDisabled)
			{
				return;
			}

			// нотификация
			var evnt = PropertyChanged;
			if (evnt != null)
			{
				evnt(this, args);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}
}
