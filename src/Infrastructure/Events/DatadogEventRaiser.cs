using System.Collections.Generic;
using Crpg.Application.Common.Interfaces.Events;
using DatadogStatsD;
using DatadogStatsD.Events;

namespace Crpg.Infrastructure.Events
{
    public class DatadogEventRaiser : IEventRaiser
    {
        private readonly DogStatsD _dogStatsD;

        public DatadogEventRaiser(DogStatsD dogStatsD) => _dogStatsD = dogStatsD;

        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<string>? tags = null)
            => _dogStatsD.RaiseEvent((AlertType)eventLevel, title, message, EventPriority.Normal, aggregationKey, tags);
    }
}