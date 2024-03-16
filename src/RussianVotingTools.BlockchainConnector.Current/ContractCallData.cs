using Google.Protobuf.Collections;

namespace RussianVotingTools.BlockchainConnector
{
	/// <summary>
	/// Быстрый доступ только к важным полям вызова контракта
	/// </summary>
	public readonly ref struct ExecuteCallContractTransactionFields
	{
		private readonly WavesEnterprise.Transaction _transaction;

		public ExecuteCallContractTransactionFields(WavesEnterprise.Transaction transaction)
		{
			_transaction = transaction;
		}

		public static implicit operator ExecuteCallContractTransactionFields(WavesEnterprise.Transaction transaction)
		{
			return new ExecuteCallContractTransactionFields(transaction);
		}

		/// <summary>
		/// Выдает результат transaction.ExecutedContractTransaction.Tx.CallContractTransaction.ContractId.ToByteArray()
		/// </summary>
		public readonly byte[] ContractID
		{
			get { return _transaction.ExecutedContractTransaction.Tx.CallContractTransaction.ContractId.ToByteArray(); }
		}

		public readonly long Timestamp
		{
			get { return _transaction.ExecutedContractTransaction.Timestamp; }
		}

		public readonly RepeatedField<global::WavesEnterprise.DataEntry> Params
		{
			get { return _transaction.ExecutedContractTransaction.Tx.CallContractTransaction.Params; }
		}

		public readonly RepeatedField<global::WavesEnterprise.DataEntry> Results
		{
			get { return _transaction.ExecutedContractTransaction.Results; }
		}
	}
}
