using System.Diagnostics.CodeAnalysis;

namespace RussianVotingTools.BlockchainConnector.Abstractions
{
	public sealed class ByteStringEqualityComparer
		: EqualityComparer<byte[]>
	{
		public override bool Equals(byte[]? x, byte[]? y)
		{
			return MemoryExtensions.SequenceEqual<byte>(x, y);
		}

		public override int GetHashCode([DisallowNull] byte[] obj)
		{
			return obj.Length < 4 ? obj.Length : BitConverter.ToInt32(obj);
		}


		public static readonly ByteStringEqualityComparer Instance = new();
	}
}
