using System;
using System.Collections.Generic;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusUpdate
    {
        public StrategusHeroViewModel Hero { get; init; } = default!;
        public IList<StrategusHeroVisibleViewModel> VisibleHeroes { get; init; } = Array.Empty<StrategusHeroVisibleViewModel>();
        public IList<StrategusSettlementPublicViewModel> VisibleSettlements { get; init; } = Array.Empty<StrategusSettlementPublicViewModel>();
    }
}
