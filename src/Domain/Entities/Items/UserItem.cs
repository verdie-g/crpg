using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Item owned by a <see cref="User"/>.
/// </summary>
public class UserItem : AuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// { 1, 2, 3 } for levels of looming.
    /// </summary>
    public bool IsBroken { get; set; }
    public User? User { get; set; }
    public Item? Item { get; set; }

    /// <summary>
    /// Characters with that item equipped.
    /// </summary>
    public List<EquippedItem> EquippedItems { get; set; } = new();
}
