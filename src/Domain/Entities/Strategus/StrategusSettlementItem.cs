using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Item owned by a settlement on Strategus. Similar to <see cref="StrategusHeroItem"/> but for <see cref="StrategusSettlement"/>s.
    /// </summary>
    public class StrategusSettlementItem : AuditableEntity
    {
        public int SettlementId { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }

        public StrategusSettlement? Settlement { get; set; }
        public Item? Item { get; set; }
    }
}
