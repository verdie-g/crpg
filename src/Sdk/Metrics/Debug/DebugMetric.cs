using System.Collections.Generic;
using System.Linq;
using Crpg.Sdk.Abstractions.Metrics;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Sdk.Metrics.Debug
{
    internal class DebugMetric : ICount, IHistogram, IGauge
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DebugMetric>();

        private readonly string _name;
        private readonly string _tagsStr;

        public DebugMetric(string name, IList<KeyValuePair<string, string>>? tags)
        {
            _name = name;
            _tagsStr = tags != null
                ? string.Join(",", tags.Select(tag => $"{tag.Key}:{tag.Value}"))
                : string.Empty;
        }

        public void Record(double value) => Logger.LogDebug($"{_name}[{_tagsStr}] record {value}");
        public void Increment(long delta) => Logger.LogDebug($"{_name}[{_tagsStr}] increment by {delta}");
        public void Decrement(long delta) => Logger.LogDebug($"{_name}[{_tagsStr}] decrement by {delta}");
        public void Dispose() { }
    }
}
