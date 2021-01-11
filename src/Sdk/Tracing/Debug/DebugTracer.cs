using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Tracing;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Sdk.Tracing.Debug
{
    internal class DebugTracer : ITracer
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DebugTracer>();

        public ITraceSpan CreateSpan(string operationName, string? resourceName = null, IEnumerable<KeyValuePair<string, string>>? tags = null)
        {
            var span = new DebugTraceSpan(operationName, resourceName, tags);
            Logger.Log(LogLevel.Debug, "Start of trace {0}", span.ToString());
            return span;
        }
    }
}
