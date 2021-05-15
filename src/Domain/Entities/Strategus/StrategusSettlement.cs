using System;
using System.Collections.Generic;
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
        /// Bannerlord scene to load when a siege happens.
        /// </summary>
        public string Scene { get; set; } = default!;

        /// <summary>
        /// Garrison of the <see cref="StrategusSettlement"/>.
        /// </summary>
        public int Troops { get; set; }

        /// <summary>See <see cref="Owner"/>.</summary>
        public int? OwnerId { get; set; }

        /// <summary>
        /// Owner of the <see cref="StrategusSettlement"/>.
        /// </summary>
        public StrategusHero? Owner { get; set; }

        /// <summary>
        /// Inventory of the <see cref="StrategusSettlement"/>. Used as a simple storage or as equipment for the
        /// <see cref="Troops"/> during sieges.
        /// </summary>
        public IList<StrategusSettlementItem> Items { get; set; } = new List<StrategusSettlementItem>();
    }
}
