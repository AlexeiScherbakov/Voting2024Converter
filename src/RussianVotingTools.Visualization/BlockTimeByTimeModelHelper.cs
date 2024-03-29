using OxyPlot;
using OxyPlot.Axes;

namespace RussianVotingTools.Visualization
{
	public sealed class BlockTimeByTimeModelHelper
		: BasePlotModelHelper
	{
		private DateTimeAxis _xAxis;
		private TimeSpanAxis _yAxis;

		protected override Axis CreateXAxis()
		{
			_xAxis = new DateTimeAxis()
			{
				Title = "Время"
			};
			return _xAxis;
		}

		protected override Axis CreateYAxis()
		{
			_yAxis = new TimeSpanAxis()
			{
				Title = "Время вычисления блока"
			};
			_yAxis.AbsoluteMinimum = TimeSpanAxis.ToDouble(TimeSpan.Zero);
			_yAxis.Minimum = TimeSpanAxis.ToDouble(TimeSpan.Zero);
			return _yAxis;
		}

		public void LoadData(string name, IEnumerable<BlockTimePoint> points)
		{
			lock (PlotModel.SyncRoot)
			{
				OxyPlot.Series.LineSeries lineSeries = new()
				{
					Title = name,
				};
				foreach (var point in points)
				{
					var x = DateTimeAxis.ToDouble(point.BlockTime);
					var y = TimeSpanAxis.ToDouble(point.BlockCalculationTime);
					lineSeries.Points.Add(new DataPoint(x, y));
				}
				PlotModel.Series.Add(lineSeries);
			}
		}


		public readonly struct BlockTimePoint
		{
			public readonly TimeSpan BlockCalculationTime;
			public readonly DateTime BlockTime;

			public BlockTimePoint(TimeSpan blockCalculationTime, DateTime blockTime)
			{
				BlockCalculationTime = blockCalculationTime;
				BlockTime = blockTime;
			}
		}
	}
}
