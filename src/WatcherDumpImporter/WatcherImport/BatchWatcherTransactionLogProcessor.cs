using System.Reactive.Disposables;
using System.Threading.Tasks.Dataflow;

namespace WatcherDumpImporter.WatcherImport
{
	public abstract class BatchWatcherTransactionLogProcessor
		: IDisposable
	{
		private BufferBlock<TransactionProcessingItem> _bufferBlock;
		private TransformBlock<TransactionProcessingItem, TransactionProcessingItem> _parseTransactionBlock;
		private BatchBlock<TransactionProcessingItem> _batchBlock;
		private ActionBlock<TransactionProcessingItem[]> _batchProcessingBlock;

		private readonly int _batchSize;

		private CompositeDisposable _disposables = new();


		private bool _completed;

		public BatchWatcherTransactionLogProcessor(int batchSize)
		{
			_batchSize = batchSize;

			_bufferBlock = new(new()
			{
				EnsureOrdered = true,
				BoundedCapacity = 3 * _batchSize
			});

			_parseTransactionBlock = new(TransactionDeserializeAction, new()
			{
				EnsureOrdered = true,
				MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
				BoundedCapacity = 2 * _batchSize
			});

			_batchBlock = new(_batchSize, new()
			{
				BoundedCapacity = 3 * _batchSize,
				EnsureOrdered = true
			});

			_batchProcessingBlock = new(TransactionBatchProcessingAction, new()
			{
				MaxDegreeOfParallelism = 1,
				BoundedCapacity = 1
			});

			var d = _bufferBlock.LinkTo(_parseTransactionBlock, new()
			{

			});
			_disposables.Add(d);

			d = _parseTransactionBlock.LinkTo(_batchBlock, new()
			{

			});
			_disposables.Add(d);

			d = _batchBlock.LinkTo(_batchProcessingBlock, new());
			_disposables.Add(d);
		}

		public void Dispose()
		{
			// Должны дождаться завершения
			if (!_completed)
			{
				throw new InvalidOperationException();
			}
			Dispose(true);
			// разбираем конвеер
			_disposables.Dispose();
			GC.SuppressFinalize(true);
		}

		protected virtual void Dispose(bool disposing)
		{
		}


		private TransactionProcessingItem TransactionDeserializeAction(TransactionProcessingItem task)
		{
			try
			{
				var tx = WavesEnterprise.Transaction.Parser.ParseFrom(task.TransactionBody);
				task.Transaction = tx;
				bool rollback = false;
				if (task.Properties.TryGetValue("event", out var ev))
				{
					if (ev == "rollback")
					{
						rollback = true;
					}
				}
				int height = int.Parse(task.Properties["height"]);
				task.Height = height;
			}
			catch (Exception e)
			{

			}
			return task;
		}


		protected abstract Task TransactionBatchProcessingAction(TransactionProcessingItem[] tasks);

		public async Task EnqueueTransactionForProcessing(TransactionProcessingItem item)
		{
			while (_bufferBlock.Count > _batchSize)
			{
				OnWaitingQueue();
				await Task.Delay(1000);
			}
			_bufferBlock.Post(item);
		}

		protected virtual void OnWaitingQueue()
		{
			Console.WriteLine("Buffer");
		}
		
		public async Task EndAndWaitForCompletion()
		{
			_bufferBlock.Complete();
			await _bufferBlock.Completion;
			_parseTransactionBlock.Complete();
			await _parseTransactionBlock.Completion;
			_batchBlock.Complete();
			await _batchBlock.Completion;
			_batchProcessingBlock.Complete();
			await _batchProcessingBlock.Completion;
			_completed = true;
		}
	}
}
