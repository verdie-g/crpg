using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleFighterApplicationViewModel : IMapFrom<StrategusBattleFighterApplication>
    {
        public int Id { get; init; }
        public StrategusHeroVisibleViewModel? Hero { get; init; }
        public StrategusBattleFighterApplicationStatus Status { get; init; }
    }
}
