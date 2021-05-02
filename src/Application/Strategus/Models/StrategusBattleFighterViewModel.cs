using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleFighterViewModel : IMapFrom<StrategusBattleFighter>
    {
        public int Id { get; init; }
        public StrategusHeroPublicViewModel? Hero { get; init; }
        public StrategusSettlementPublicViewModel? Settlement { get; init; }
        public StrategusBattleSide Side { get; init; }
    }
}
