using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusBattle : AuditableEntity
    {
        public int Id { get; set; }
        public StrategusUser Attacker { get; set; } = default!;
        public StrategusUser Defender { get; set; } = default!; // TODO: How to attack settlements?
    }
}
