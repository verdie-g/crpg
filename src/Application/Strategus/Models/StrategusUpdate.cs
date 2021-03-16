using System;
using System.Collections.Generic;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusUpdate
    {
        public StrategusUserViewModel User { get; set; } = default!;
        public IList<StrategusUserPublicViewModel> VisibleUsers { get; set; } = Array.Empty<StrategusUserPublicViewModel>();
        public IList<StrategusSettlementViewModel> VisibleSettlements { get; set; } = Array.Empty<StrategusSettlementViewModel>();
    }
}
