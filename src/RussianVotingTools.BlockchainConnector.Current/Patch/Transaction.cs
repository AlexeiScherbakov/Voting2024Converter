using RussianVotingTools.BlockchainConnector.ObjectModel;

// подкостыливание объектов созданных Grpc обозначающих транзакции и события с ними

namespace WavesEnterprise
{
	partial class Transaction
	{

		public byte[] GetTransactionId()
		{
			return ((ITransactionWithId) transaction_).Id.ToByteArray();
		}

		public long GetTimestamp()
		{
			return ((ITransactionWithId) transaction_).Timestamp;
		}
	}

	partial class GenesisTransaction
		: ITransactionWithId
	{

	}

	partial class GenesisPermitTransaction
		: ITransactionWithId
	{

	}

	partial class GenesisRegisterNodeTransaction
		: ITransactionWithId
	{

	}

	partial class RegisterNodeTransaction
		: ITransactionWithId
	{

	}

	partial class CreateAliasTransaction
		: ITransactionWithId
	{

	}

	partial class IssueTransaction
		: ITransactionWithId
	{

	}

	partial class ReissueTransaction
		: ITransactionWithId
	{

	}

	partial class BurnTransaction
		: ITransactionWithId
	{

	}

	partial class LeaseTransaction
		: ITransactionWithId
	{

	}

	partial class LeaseCancelTransaction
		: ITransactionWithId
	{

	}

	partial class SponsorFeeTransaction
		: ITransactionWithId
	{

	}

	partial class SetAssetScriptTransaction
		: ITransactionWithId
	{

	}

	partial class DataTransaction
		: ITransactionWithId
	{

	}

	partial class TransferTransaction
		: ITransactionWithId
	{

	}

	partial class MassTransferTransaction
		: ITransactionWithId
	{

	}

	partial class PermitTransaction
		: ITransactionWithId
	{

	}

	partial class CreatePolicyTransaction
		: ITransactionWithId
	{

	}

	partial class UpdatePolicyTransaction
		: ITransactionWithId
	{

	}

	partial class PolicyDataHashTransaction
		: ITransactionWithId
	{

	}

	partial class CreateContractTransaction
		: ITransactionWithId
	{

	}

	partial class CallContractTransaction
		: ITransactionWithId
	{

	}

	partial class ExecutedContractTransaction
		: ITransactionWithId
	{

	}

	partial class DisableContractTransaction
		: ITransactionWithId
	{

	}

	partial class UpdateContractTransaction
		: ITransactionWithId
	{

	}

	partial class SetScriptTransaction
		: ITransactionWithId
	{

	}

	partial class AtomicTransaction
		: ITransactionWithId
	{

	}
}
