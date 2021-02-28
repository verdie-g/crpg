using NetTopologySuite.Geometries;

namespace Crpg.GameMod.Api.Models.Strategus
{
    internal class CrpgStrategusSettlementCreation
    {
        public string Name { get; set; } = default!;
        public CrpgStrategusSettlementType Type { get; set; }
        public CrpgCulture Culture { get; set; }
        public Point Position { get; set; } = default!;
        public string Scene { get; set; } = default!;
    }

    // Copy of Crpg.Domain.Entities.Strategus.StrategusSettlementType.
    internal enum CrpgStrategusSettlementType
    {
        Village,
        Castle,
        Town,
    }
}
