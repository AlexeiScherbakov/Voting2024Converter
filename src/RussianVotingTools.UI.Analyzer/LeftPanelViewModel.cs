using System.Collections.ObjectModel;
using System.Reactive.Linq;

using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.UI.Abstractions;
using RussianVotingTools.UI.Analyzer.ViewModels;
using RussianVotingTools.UI.Analyzer.Windows;

namespace RussianVotingTools.UI.Analyzer
{
	public sealed class LeftPanelViewModel
		: ReactiveObject, IDisposable
	{
		private SourceCache<ElectionTimelineViewModel, long> _sourceCache;

		private ReadOnlyObservableCollection<ElectionTimelineViewModel> _electionTimelines;

		private IMainDatabaseConnection _mainDatabaseConnection;

		private IDisposable _subscription;

		public LeftPanelViewModel(IMainDatabaseConnection mainDatabaseConnection)
		{
			_mainDatabaseConnection = mainDatabaseConnection;

			_sourceCache = new SourceCache<ElectionTimelineViewModel, long>(x => x.Id);

			var subscription = _sourceCache.Connect()
				.Sort(SortExpressionComparer<ElectionTimelineViewModel>.Descending(t => t.PlannedStartTime))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Bind(out _electionTimelines)
				.Subscribe();
		}

		public void Dispose()
		{
			_subscription.Dispose();
		}

		public ReadOnlyObservableCollection<ElectionTimelineViewModel> ElectionTimelines
		{
			get { return _electionTimelines; }
		}

		public async Task UpdateAsync()
		{
			var allTimelines = await _mainDatabaseConnection.ElectionTimeline.GetAllAsync();
			var allObservations = await _mainDatabaseConnection.ElectionObservation.GetAllAsync();

			Dictionary<long, ElectionTimelineViewModel> timelines = new();
			foreach (var timeline in allTimelines)
			{
				ElectionTimelineViewModel timelineVm = new(timeline.Id)
				{
					Name = timeline.Data.Name,
					PlannedStartTime = timeline.Data.PlannedStartTime,
					PlannedEndTime = timeline.Data.PlannedEndTime,
					StartTime = timeline.Data.StartTime,
					EndTime = timeline.Data.EndTime
				};
				timelines.Add(timeline.Id, timelineVm);
			}
			foreach (var observation in allObservations)
			{
				ElectionObservationViewModel observationVm = new(observation.Id)
				{
					Name = observation.Data.Name
				};
				if (timelines.TryGetValue(observation.ElectionTimelineId, out var timelineVm))
				{
					timelineVm.ElectionObservations.Add(observationVm);
				}
			}

			Dictionary<long, ElectionObservationViewModel> observations = new();

			_sourceCache.Edit(update =>
			{
				update.Clear();

				foreach (var vm in timelines)
				{
					_sourceCache.AddOrUpdate(vm.Value);
				}
			});
		}

		#region ElectionTimeline

		public async Task CreateElectionTimeline(ElectionTimelineViewModel electionTimeline)
		{
			var id = await _mainDatabaseConnection.ElectionTimeline.CreateAsync(new ElectionTimelineData()
			{
				Name = electionTimeline.Name,
				PlannedStartTime = electionTimeline.PlannedStartTime,
				PlannedEndTime = electionTimeline.PlannedEndTime,
				StartTime = electionTimeline.StartTime,
				EndTime = electionTimeline.EndTime
			});

			_sourceCache.Edit(update =>
			{
				_sourceCache.AddOrUpdate(new ElectionTimelineViewModel(id)
				{
					Name = electionTimeline.Name,
					PlannedStartTime = electionTimeline.PlannedStartTime,
					PlannedEndTime = electionTimeline.PlannedEndTime,
					StartTime = electionTimeline.StartTime,
					EndTime = electionTimeline.EndTime
				});
			});
		}

		#endregion

		#region ElectionTimeline Commands

		public sealed class CreateElectionTimelineCommandImpl
			: SiteCommand
		{
			private readonly LeftPanelViewModel _root;

			internal CreateElectionTimelineCommandImpl(LeftPanelViewModel root)
			{
				_root = root;
			}
			public override async Task ExecuteAsync(ICommandSite site)
			{
				using var op = site.UIScopeFactory.CreateUIScope();

				var window = await op.WindowFactory.CreateWindowAsync<IElectionTimelineEditor>();

				ElectionTimelineViewModel vm = new(0);
				window.EditableObject = vm;

				if (await window.Chrome.ShowDialog() != true)
				{
					return;
				}

				await _root.CreateElectionTimeline(vm);
			}
		}

		#endregion
	}
}
