using System.Collections.Generic;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Item owned by a user.
    /// </summary>
    public class OwnedItem : AuditableEntity
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }

        public User? User { get; set; }
        public Item? Item { get; set; }

        /// <summary>
        /// Characters with that item equipped.
        /// </summary>
        public IList<EquippedItem> EquippedItems { get; set; } = new List<EquippedItem>();
    }
}
