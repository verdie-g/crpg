using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class EquippedItemViewModel : IMapFrom<EquippedItem>
    {
        public ItemViewModel Item { get; set; } = default!;
        public ItemSlot Slot { get; set; }
    }
}
