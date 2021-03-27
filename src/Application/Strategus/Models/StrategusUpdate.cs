using System;
using System.Collections.Generic;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusUpdate
    {
        public StrategusHeroViewModel Hero { get; set; } = default!;
        public IList<StrategusHeroPublicViewModel> VisibleHeroes { get; set; } = Array.Empty<StrategusHeroPublicViewModel>();
        public IList<StrategusSettlementViewModel> VisibleSettlements { get; set; } = Array.Empty<StrategusSettlementViewModel>();
    }
}
