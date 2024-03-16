using System.Diagnostics.CodeAnalysis;

namespace RussianVotingTools.DataAbstractions
{
	/// <summary>
	/// Конвертер из объекта в БД в промежуточные объекты (DTO like)
	/// </summary>
	/// <typeparam name="TDatabaseObject">Объект в БД (ORM)</typeparam>
	/// <typeparam name="TDataObject">Объект DTO без связей</typeparam>
	/// <typeparam name="TStorageDataObject">Объект DTO со связями/ссылками на другие объекты</typeparam>
	public interface IDatabaseObjectToDataObjectConverter<TDatabaseObject, TDataObject, TStorageDataObject>
	{
		/// <summary>
		/// Переводит БД объект в DTO без связей
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		static abstract TDataObject ToData(TDatabaseObject dbObject);

		/// <summary>
		/// Переводит БД объект в DTO со связями
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		[return: NotNullIfNotNull(nameof(dbObject))]
		static abstract TStorageDataObject? ToStorageData(TDatabaseObject? dbObject);


		static abstract void CopyToDatabaseObject(TDataObject source, TDatabaseObject destination);

		static abstract void CopyToDataObject(TDatabaseObject source, TDataObject destination);
	}
}
