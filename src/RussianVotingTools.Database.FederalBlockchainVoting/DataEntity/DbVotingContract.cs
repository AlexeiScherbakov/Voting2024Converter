using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianVotingTools.Database.FederalBlockchainVoting.DataEntity
{
	/// <summary>
	/// Подробное описание контракта голосования
	/// </summary>
	public class DbVotingContract
		: DbIdObject
	{
		public virtual byte[] ContractId { get; set; }
		public virtual byte[] ExecuteTxId { get; set; }
		public virtual string? Name { get; set; }
		public virtual string? Image { get; set; }

		public virtual string? ImageHash { get; set; }

		public virtual string? PollId { get; set; }

		public virtual string? CommissionId { get; set; }

		public virtual string? BulletinHash { get; set; }

		public virtual long Timestamp { get; set; }

		public virtual bool? IsRevoteBlocked { get; set; }
	}
}
