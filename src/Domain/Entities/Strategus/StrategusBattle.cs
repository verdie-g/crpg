using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusBattle : AuditableEntity
    {
        public int Id { get; set; }
        public StrategusHero Attacker { get; set; } = default!;
        public StrategusHero Defender { get; set; } = default!; // TODO: How to attack settlements?
    }
}
