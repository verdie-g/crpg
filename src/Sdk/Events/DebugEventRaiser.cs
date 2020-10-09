using System;
using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Crpg.Sdk.Events
{
    public class DebugEventRaiser : IEventRaiser
    {
        private readonly ILogger<DebugEventRaiser> _logger;

        public DebugEventRaiser(ILogger<DebugEventRaiser> logger)
        {
            _logger = logger;
        }

        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<string>? tags = null)
        {
            string tagsStr = tags != null ? string.Join(",", tags) : string.Empty;
            _logger.Log(EventToLogLevel(eventLevel), $"{title}: {message} [{tagsStr}]");
        }

        private LogLevel EventToLogLevel(EventLevel eventLevel)
        {
            return eventLevel switch
            {
                EventLevel.Info => LogLevel.Information,
                EventLevel.Success => LogLevel.Information,
                EventLevel.Warning => LogLevel.Warning,
                EventLevel.Error => LogLevel.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(eventLevel)),
            };
        }
    }
}
