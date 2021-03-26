using System.Collections.Generic;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusBattle : AuditableEntity
    {
        public int Id { get; set; }
        public StrategusBattleStatus Status { get; set; }

        /// <summary>
        /// The id of the attacked <see cref="StrategusSettlement"/>. Can be null if the <see cref="StrategusBattle"/>
        /// is between <see cref="StrategusHero"/>es.
        /// </summary>
        public int? AttackedSettlementId { get; set; }

        /// <summary>See <see cref="AttackedSettlementId"/>.</summary>
        public StrategusSettlement? AttackedSettlement { get; set; }
        public List<StrategusBattleFighter> Fighters { get; set; } = new List<StrategusBattleFighter>();
    }
}
