using System;
using System.Collections.Generic;
using Crpg.Application.Heroes.Models;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusUpdate
    {
        public HeroViewModel Hero { get; init; } = default!;
        public IList<HeroVisibleViewModel> VisibleHeroes { get; init; } = Array.Empty<HeroVisibleViewModel>();
        public IList<SettlementPublicViewModel> VisibleSettlements { get; init; } = Array.Empty<SettlementPublicViewModel>();
    }
}
