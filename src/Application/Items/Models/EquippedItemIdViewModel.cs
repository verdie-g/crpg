using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public record EquippedItemIdViewModel
    {
        public int? ItemId { get; init; }
        public ItemSlot Slot { get; init; }
    }
}
