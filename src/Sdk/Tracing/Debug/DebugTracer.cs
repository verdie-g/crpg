using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace Crpg.Sdk.Tracing.Debug
{
    internal class DebugTracer : ITracer
    {
        private readonly ILogger<DebugTracer> _logger;

        public DebugTracer(ILogger<DebugTracer> logger) => _logger = logger;

        public ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>>? tags = null)
        {
            var span = new DebugTraceSpan(name, tags, _logger);
            _logger.Log(LogLevel.Debug, "Start of trace {0}", span.ToString());
            return span;
        }
    }
}
