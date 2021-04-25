using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattlePublicViewModel
    {
        public int Id { get; init; }
        public StrategusBattlePhase Phase { get; set; }
        public StrategusBattleFighterPublicViewModel Attacker { get; init; } = default!;
        public int AttackerTotalTroops { get; init; }
        public StrategusBattleFighterPublicViewModel? Defender { get; init; }
        public StrategusSettlementPublicViewModel? SettlementDefender { get; init; }
        public int DefenderTotalTroops { get; init; }
    }
}
