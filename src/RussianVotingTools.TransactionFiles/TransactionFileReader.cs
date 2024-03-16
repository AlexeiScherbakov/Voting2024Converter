using System.Text;
using System.Text.Json;

namespace Voting2021.FilesUtils
{


	public class TransactionFileReader
		: IDisposable
	{
		private FileStream _stream;
		public TransactionFileReader(string fileName)
		{
			_stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, FileOptions.SequentialScan);
		}

		~TransactionFileReader()
		{
			Close();
		}

		public void Dispose()
		{
			Close();
			GC.SuppressFinalize(this);
		}

		private void Close()
		{
			//_accessor.Dispose();
			//_stream.SafeMemoryMappedViewHandle.ReleasePointer();
			_stream?.Dispose();
			//_memoryMappedFile?.Dispose();
		}

		public bool Eof
		{
			get { return _stream.Position >= _stream.Length; }
		}

		public void SetPosition(long position)
		{
			_stream.Position = position;
		}


		public (string, byte[] binaryTransaction, Dictionary<string, string> properties,long offset) ReadRecord()
		{
			if (!SearchStartSequence())
			{
				return (null, null, null, 0);
			}

			var bytic = _stream.ReadByte();
			switch (bytic)
			{
				case 0xBE:
					{
						long currentOffset = _stream.Position;
						using var binaryReader = new BinaryReader(_stream, Encoding.UTF8, true);
						int size = binaryReader.Read7BitEncodedInt();
						var value = binaryReader.ReadBytes(size);
						var dictionary = ReadDictionary();
						return (null, value, dictionary, currentOffset - 4);
					}
					break;
				default:
					break;
			}
			return (null, null, null, 0);
		}

		private bool SearchStartSequence()
		{
			int bytic;
			do
			{
				bytic = _stream.ReadByte();
				if (bytic != 0xCA)
				{
					continue;
				}
				bytic = _stream.ReadByte();
				if (bytic != 0xFE)
				{
					continue;
				}
				bytic = _stream.ReadByte();
				if (bytic != 0xBA)
				{
					continue;
				}
				return true;
			} while (bytic >= 0);
			return false;
		}


		private Dictionary<string, string> ReadDictionary()
		{
			int counter = 0;
			int pagePointer = 0;

			bool insideString = false;
			bool lastWasEscape = false;

			MemoryStream m = new MemoryStream(1024);
			do
			{
				var ch = _stream.ReadByte();
				m.WriteByte((byte) ch);
				switch (ch)
				{
					case '{':
						if (!insideString)
						{
							counter++;
						}
						break;
					case '}':
						if (!insideString)
						{
							counter--;
						}
						break;
					case '"':
						if (insideString)
						{
							if ((pagePointer > 0) && lastWasEscape)
							{
								//ignore
							}
							else
							{
								insideString = false;
							}
						}
						else
						{
							insideString = true;
						}
						break;
				}
				pagePointer++;
			} while ((counter > 0) && (_stream.Position < _stream.Length));
			m.Position = 0;
			Memory<byte> buffer = m.GetBuffer();
			var slicedBuffer = buffer.Slice(0, (int) m.Length);
			return JsonSerializer.Deserialize<Dictionary<string, string>>(slicedBuffer.Span);
		}
	}
}
