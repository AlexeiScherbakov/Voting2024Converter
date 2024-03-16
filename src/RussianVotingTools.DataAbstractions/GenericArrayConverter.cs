namespace RussianVotingTools.DataAbstractions
{
	public static class GenericArrayConverter
	{
		public static TConverted[] ArrayConvert<TObject, TConverted>(this TObject[] array, Func<TObject, TConverted> converterFunc)
		{
			TConverted[] ret = new TConverted[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				ret[i] = converterFunc(array[i]);
			}
			return ret;
		}

		public static TConverted[] ConvertToArray<TList, TObject, TConverted>(this TList list, Func<TObject, TConverted> converterFunc)
			where TList : ICollection<TObject>
		{
			TConverted[] ret = new TConverted[list.Count];
			int i = 0;
			foreach (var obj in list)
			{
				ret[i] = converterFunc(obj);
				i++;
			}
			return ret;
		}

		public static TConverted[] ConvertToArray<TObject, TConverted>(this List<TObject> list, Func<TObject, TConverted> converterFunc)
		{
			TConverted[] ret = new TConverted[list.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = converterFunc(list[i]);
			}
			return ret;
		}
	}
}
