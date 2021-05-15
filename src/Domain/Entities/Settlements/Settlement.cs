using System.Collections.Generic;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.Heroes;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Settlements
{
    /// <summary>
    /// Represents a settlement (village, castle, town) on Strategus.
    /// </summary>
    public class Settlement : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public SettlementType Type { get; set; }
        public Culture Culture { get; set; }

        /// <summary>
        /// Region in which the <see cref="Settlement"/> is located.
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
        /// Garrison of the <see cref="Settlement"/>.
        /// </summary>
        public int Troops { get; set; }

        /// <summary>See <see cref="Owner"/>.</summary>
        public int? OwnerId { get; set; }

        /// <summary>
        /// Owner of the <see cref="Settlement"/>.
        /// </summary>
        public Hero? Owner { get; set; }

        /// <summary>
        /// Inventory of the <see cref="Settlement"/>. Used as a simple storage or as equipment for the
        /// <see cref="Troops"/> during sieges.
        /// </summary>
        public IList<SettlementItem> Items { get; set; } = new List<SettlementItem>();
    }
}
