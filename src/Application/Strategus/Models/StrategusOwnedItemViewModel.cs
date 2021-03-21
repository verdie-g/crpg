using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusOwnedItemViewModel : IMapFrom<StrategusOwnedItem>
    {
        public ItemViewModel Item { get; set; } = default!;
        public int Count { get; set; }
    }
}
