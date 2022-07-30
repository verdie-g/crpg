using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record EquippedItemIdViewModel
{
    public ItemSlot Slot { get; set; }
    public int? UserItemId { get; set; }
}
