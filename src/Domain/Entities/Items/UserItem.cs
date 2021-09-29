using System.Collections.Generic;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Item owned by a <see cref="User"/>.
    /// </summary>
    public class UserItem : AuditableEntity
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }

        public User? User { get; set; }
        public Item? Item { get; set; }

        /// <summary>
        /// Characters with that item equipped.
        /// </summary>
        public List<EquippedItem> EquippedItems { get; set; } = new();
    }
}
