using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusSettlementCreation
    {
        public string Name { get; init; } = string.Empty;
        public StrategusSettlementType Type { get; init; }
        public Culture Culture { get; init; }
        public Point Position { get; init; } = default!;
        public string Scene { get; init; } = string.Empty;
    }
}
