using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Represents a settlement (village, castle, town) on Strategus.
    /// </summary>
    public class StrategusSettlement : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public StrategusSettlementType Type { get; set; }
        public Culture Culture { get; set; }

        /// <summary>
        /// Region in which the <see cref="StrategusSettlement"/> is located.
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// Position of the settlement on the strategus map.
        /// </summary>
        public Point Position { get; set; } = default!;

        /// <summary>
        /// Bannelord scene to load when a siege happens.
        /// </summary>
        public string Scene { get; set; } = default!;

        /// <summary>See <see cref="Owner"/>.</summary>
        public int? OwnerId { get; set; }

        /// <summary>
        /// Owner of the <see cref="StrategusSettlement"/>.
        /// </summary>
        public StrategusUser? Owner { get; set; }
    }
}
