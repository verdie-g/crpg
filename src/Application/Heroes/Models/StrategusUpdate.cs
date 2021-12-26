using Crpg.Application.Battles.Models;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Heroes.Models;

public record StrategusUpdate
{
    public HeroViewModel Hero { get; init; } = default!;
    public IList<HeroVisibleViewModel> VisibleHeroes { get; init; } = Array.Empty<HeroVisibleViewModel>();
    public IList<SettlementPublicViewModel> VisibleSettlements { get; init; } = Array.Empty<SettlementPublicViewModel>();
    public IList<BattleViewModel> VisibleBattles { get; init; } = Array.Empty<BattleViewModel>();
}
