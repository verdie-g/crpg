using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class EquippedItemIdViewModel
    {
        public int? ItemId { get; set; }
        public ItemSlot Slot { get; set; }
    }
}
