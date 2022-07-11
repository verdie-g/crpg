using Crpg.Application.Battles.Models;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Parties.Models;

public record StrategusUpdate
{
    public PartyViewModel Party { get; init; } = default!;
    public IList<PartyVisibleViewModel> VisibleParties { get; init; } = Array.Empty<PartyVisibleViewModel>();
    public IList<SettlementPublicViewModel> VisibleSettlements { get; init; } = Array.Empty<SettlementPublicViewModel>();
    public IList<BattleViewModel> VisibleBattles { get; init; } = Array.Empty<BattleViewModel>();
}
