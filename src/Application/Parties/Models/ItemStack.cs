using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View of a <see cref="PartyItem"/> or <see cref="SettlementItem"/>.
/// </summary>
public record ItemStack : IMapFrom<PartyItem>, IMapFrom<SettlementItem>
{
    public ItemViewModel Item { get; init; } = default!;
    public int Count { get; init; }
}
