using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Application.Heroes.Models;

/// <summary>
/// View of a <see cref="HeroItem"/> or <see cref="SettlementItem"/>.
/// </summary>
public record ItemStack : IMapFrom<HeroItem>, IMapFrom<SettlementItem>
{
    public ItemViewModel Item { get; init; } = default!;
    public int Count { get; init; }
}
