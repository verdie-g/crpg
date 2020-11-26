using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Sdk.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Crpg.Sdk.Events
{
    public class DebugEventService : IEventService
    {
        private readonly ILogger<DebugEventService> _logger;

        public DebugEventService(ILogger<DebugEventService> logger)
        {
            _logger = logger;
        }

        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<KeyValuePair<string, string>>? tags = null)
        {
            string tagsStr = tags != null
                ? string.Join(",", tags.Select(tag => $"{tag.Key}:{tag.Value}"))
                : string.Empty;
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
