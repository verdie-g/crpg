using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusSettlementCreation
    {
        public string Name { get; set; } = default!;
        public StrategusSettlementType Type { get; set; }
        public Culture Culture { get; set; }
        public Point Position { get; set; } = default!;
        public string Scene { get; set; } = default!;
    }
}
