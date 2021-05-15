using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Heroes
{
    /// <summary>
    /// Item owned by a hero on Strategus. Similar to <see cref="SettlementItem"/> but for <see cref="Hero"/>.
    /// </summary>
    public class HeroItem : AuditableEntity
    {
        public int HeroId { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }

        public Hero? Hero { get; set; }
        public Item? Item { get; set; }
    }
}
