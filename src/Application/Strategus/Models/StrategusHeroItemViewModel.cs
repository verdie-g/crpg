using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusHeroItemViewModel : IMapFrom<StrategusHeroItem>
    {
        public ItemViewModel Item { get; init; } = default!;
        public int Count { get; init; }
    }
}
