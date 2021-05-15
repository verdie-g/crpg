using Crpg.Application.Common.Mappings;
using Crpg.Application.Heroes.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models
{
    public record BattleFighterViewModel : IMapFrom<BattleFighter>
    {
        public int Id { get; init; }
        public HeroPublicViewModel? Hero { get; init; }
        public SettlementPublicViewModel? Settlement { get; init; }
        public BattleSide Side { get; init; }
    }
}
