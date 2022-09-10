using Crpg.Domain.Common;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Item equipped by a character.
/// </summary>
public class EquippedItem : AuditableEntity
{
    public int CharacterId { get; set; }
    public ItemSlot Slot { get; set; }
    public int UserItemId { get; set; }

    public Character? Character { get; set; }
    public UserItem? UserItem { get; set; }
}
