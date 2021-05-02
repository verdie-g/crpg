using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleFighterPublicViewModel : IMapFrom<StrategusBattleFighter>
    {
        public int Id { get; init; }
        public StrategusHeroPublicViewModel? Hero { get; init; }
        public StrategusSettlementPublicViewModel? Settlement { get; init; }
    }
}
