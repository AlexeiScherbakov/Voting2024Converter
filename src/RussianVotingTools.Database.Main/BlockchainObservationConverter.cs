using System.Diagnostics.CodeAnalysis;

using RussianVotingTools.DataAbstractions;
using RussianVotingTools.Database.Main.Abstractions;
using RussianVotingTools.Database.Main.DataEntity;

namespace RussianVotingTools.Database.Main
{
	internal sealed class BlockchainObservationConverter
		: IDatabaseObjectToDataObjectConverter<DbBlockchainObservation, FedBlockchainObservationData, FedBlockchainObservationStorageData>
	{
		private BlockchainObservationConverter()
		{

		}

		/// <inheritdoc/>
		public static void CopyToDatabaseObject(FedBlockchainObservationData source, DbBlockchainObservation destination)
		{
			destination.Name = source.Name;
			destination.Uid = source.Uid;
		}

		/// <inheritdoc/>
		public static void CopyToDataObject(DbBlockchainObservation source, FedBlockchainObservationData destination)
		{
			destination.Name = source.Name;
			destination.Uid = source.Uid;
		}

		/// <inheritdoc/>
		public static FedBlockchainObservationData ToData(DbBlockchainObservation dbObject)
		{
			FedBlockchainObservationData ret = new();
			CopyToDataObject(dbObject, ret);
			return ret;
		}

		/// <inheritdoc/>
		[return: NotNullIfNotNull("dbObject")]
		public static FedBlockchainObservationStorageData? ToStorageData(DbBlockchainObservation? dbObject)
		{
			if (dbObject is null)
			{
				return null;
			}
			FedBlockchainObservationStorageData ret = new()
			{
				Id = dbObject.ID,
				Data = ToData(dbObject)
			};
			return ret;
		}
	}
}
