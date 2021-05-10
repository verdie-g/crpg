using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Item owned by a hero on Strategus. Similar to <see cref="StrategusSettlementItem"/> but for <see cref="StrategusHero"/>.
    /// </summary>
    public class StrategusHeroItem : AuditableEntity
    {
        public int HeroId { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }

        public StrategusHero? Hero { get; set; }
        public Item? Item { get; set; }
    }
}
