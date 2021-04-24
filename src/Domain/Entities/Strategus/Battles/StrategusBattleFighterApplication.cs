using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Application to join a <see cref="StrategusBattle"/> during the <see cref="StrategusBattlePhase.Preparation"/>
    /// phase.
    /// </summary>
    public class StrategusBattleFighterApplication : AuditableEntity
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public int HeroId { get; set; }
        public StrategusBattleFighterApplicationStatus Status { get; set; }

        public StrategusBattle? Battle { get; set; }
        public StrategusHero? Hero { get; set; }
    }
}
