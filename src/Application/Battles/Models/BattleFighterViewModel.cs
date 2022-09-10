using Crpg.Application.Common.Mappings;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleFighterViewModel : IMapFrom<BattleFighter>
{
    public int Id { get; init; }
    public PartyPublicViewModel? Party { get; init; }
    public SettlementPublicViewModel? Settlement { get; init; }
    public BattleSide Side { get; init; }
    public bool Commander { get; init; }
}
