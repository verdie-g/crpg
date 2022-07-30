using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record EquippedItemViewModel : IMapFrom<EquippedItem>
{
    public ItemSlot Slot { get; init; }
    public UserItemViewModel UserItem { get; init; } = default!;
}
