using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusSettlementPublicViewModel : IMapFrom<StrategusSettlement>
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public StrategusSettlementType Type { get; set; }
        public Point Position { get; set; } = default!;
        public Culture Culture { get; set; }
        public Region Region { get; set; }
        public StrategusHeroPublicViewModel? Owner { get; set; }
    }
}
