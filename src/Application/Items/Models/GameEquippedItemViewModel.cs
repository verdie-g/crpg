using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameEquippedItemViewModel : IMapFrom<EquippedItem>
{
    public ItemSlot Slot { get; init; }
    public GameUserItemViewModel UserItem { get; init; } = default!;
}
