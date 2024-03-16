using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NHibernate.Event.Default;

using RussianVotingTools.Database.FederalBlockchainVoting;

using ZstdSharp.Unsafe;

namespace WatcherDumpImporter.WatcherImport
{
	internal sealed class IdGenerator
	{
		private readonly object _lock = new();
		private readonly Dictionary<Type, long> _counters = new();

		public IdGenerator()
		{

		}


		public long GetNextId<Type>()
			where Type : DbIdObject
		{
			long ret = -1;
			lock (_lock)
			{
				if (_counters.TryGetValue(typeof(Type),out var value))
				{
					ret = value + 1;
					_counters[typeof(Type)] = ret;
				}
				else
				{
					_counters[typeof(Type)] = 1;
					ret = 1;
				}
			}
			return ret;
		}

	}
}
