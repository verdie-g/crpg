using System.Collections.Generic;
using Crpg.Application.Common.Interfaces.Tracing;
using Microsoft.Extensions.Logging;

namespace Crpg.Infrastructure.Tracing.Debug
{
    internal class DebugTracer : ITracer
    {
        private readonly ILogger<DebugTracer> _logger;

        public DebugTracer(ILogger<DebugTracer> logger) => _logger = logger;

        public ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>> tags)
        {
            var span = new DebugTraceSpan(name, tags, _logger);
            _logger.Log(LogLevel.Debug, "Start of trace {0}", span.ToString());
            return span;
        }
    }
}
