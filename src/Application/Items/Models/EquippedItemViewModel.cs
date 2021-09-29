using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public record EquippedItemViewModel : IMapFrom<EquippedItem>
    {
        public ItemViewModel Item { get; init; } = default!;
        public ItemSlot Slot { get; init; }
    }
}
