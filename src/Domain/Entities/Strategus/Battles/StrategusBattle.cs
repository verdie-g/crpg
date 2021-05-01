using System.Collections.Generic;
using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus.Battles
{
    public class StrategusBattle : AuditableEntity
    {
        public int Id { get; set; }
        public StrategusBattlePhase Phase { get; set; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;

        /// <summary>
        /// The id of the attacked <see cref="StrategusSettlement"/>. Can be null if the <see cref="StrategusBattle"/>
        /// is between <see cref="StrategusHero"/>es.
        /// </summary>
        public int? AttackedSettlementId { get; set; }

        /// <summary>See <see cref="AttackedSettlementId"/>.</summary>
        public StrategusSettlement? AttackedSettlement { get; set; }
        public List<StrategusBattleFighter> Fighters { get; set; } = new();
        public List<StrategusBattleFighterApplication> FighterApplications { get; set; } = new();
        public List<StrategusBattleMercenary> Mercenaries { get; set; } = new();
        public List<StrategusBattleMercenaryApplication> MercenaryApplications { get; set; } = new();
    }
}
