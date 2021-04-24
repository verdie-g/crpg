using Crpg.Domain.Common;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Application to join a <see cref="StrategusBattle"/> during the <see cref="StrategusBattlePhase.Preparation"/>
    /// phase.
    /// </summary>
    public class StrategusBattleMercenaryApplication : AuditableEntity
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public int CharacterId { get; set; }
        public StrategusBattleMercenaryApplicationStatus Status { get; set; }

        public StrategusBattle? Battle { get; set; }
        public Character? Character { get; set; }
    }
}
