using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Metrics;
using Microsoft.Extensions.Logging;

namespace Crpg.Sdk.Metrics.Debug
{
    internal class DebugMetric : ICount, IHistogram, IGauge
    {
        private readonly string _name;
        private readonly string _tagsStr;
        private readonly ILogger _logger;

        public DebugMetric(string name, IList<string>? tags, ILogger logger)
        {
            _name = name;
            _tagsStr = tags != null ? string.Join(",", tags) : string.Empty;
            _logger = logger;
        }

        public void Record(double value) => _logger.LogDebug($"{_name}[{_tagsStr}] record {value}");
        public void Increment(long delta) => _logger.LogDebug($"{_name}[{_tagsStr}] increment by {delta}");
        public void Decrement(long delta) => _logger.LogDebug($"{_name}[{_tagsStr}] decrement by {delta}");
        public void Dispose() { }
    }
}
