using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusSettlementPublicViewModel : IMapFrom<StrategusSettlement>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public StrategusSettlementType Type { get; init; }
        public Point Position { get; init; } = default!;
        public Culture Culture { get; init; }
        public Region Region { get; init; }
    }
}
