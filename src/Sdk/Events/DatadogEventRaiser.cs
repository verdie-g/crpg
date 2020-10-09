using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Events;
using DatadogStatsD;
using DatadogStatsD.Events;

namespace Crpg.Sdk.Events
{
    public class DatadogEventRaiser : IEventRaiser
    {
        private readonly DogStatsD _dogStatsD;

        public DatadogEventRaiser(DogStatsD dogStatsD) => _dogStatsD = dogStatsD;

        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<string>? tags = null)
        {
            // Datadog agent doesn't accept empty messages (https://github.com/DataDog/datadog-agent/issues/6054)
            message = message.Length == 0 ? " " : message;
            _dogStatsD.RaiseEvent((AlertType)eventLevel, title, message, EventPriority.Normal, aggregationKey, tags);
        }
    }
}
