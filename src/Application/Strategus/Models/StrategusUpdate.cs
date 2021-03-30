using System;
using System.Collections.Generic;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusUpdate
    {
        public StrategusHeroViewModel Hero { get; set; } = default!;
        public IList<StrategusHeroVisibleViewModel> VisibleHeroes { get; set; } = Array.Empty<StrategusHeroVisibleViewModel>();
        public IList<StrategusSettlementPublicViewModel> VisibleSettlements { get; set; } = Array.Empty<StrategusSettlementPublicViewModel>();
    }
}
