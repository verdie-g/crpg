using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Strategus.Models
{
    /// <summary>
    /// View of a <see cref="StrategusHeroItem"/> or <see cref="StrategusSettlementItem"/>.
    /// </summary>
    public record ItemStack : IMapFrom<StrategusHeroItem>, IMapFrom<StrategusSettlementItem>
    {
        public ItemViewModel Item { get; init; } = default!;
        public int Count { get; init; }
    }
}
