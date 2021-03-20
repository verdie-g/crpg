using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Item owned by a user on Strategus.
    /// </summary>
    public class StrategusOwnedItem : AuditableEntity
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }

        public StrategusHero? User { get; set; }
        public Item? Item { get; set; }
    }
}
