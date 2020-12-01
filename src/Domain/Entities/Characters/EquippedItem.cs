using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Characters
{
    /// <summary>
    /// Item equipped by a character.
    /// </summary>
    public class EquippedItem : AuditableEntity
    {
        public int CharacterId { get; set; }
        public ItemSlot Slot { get; set; }
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public Character? Character { get; set; }
        public Item? Item { get; set; }
        public UserItem? UserItem { get; set; }
    }
}
