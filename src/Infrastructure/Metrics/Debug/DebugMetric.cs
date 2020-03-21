using Crpg.Application.Common.Interfaces.Metrics;
using Microsoft.Extensions.Logging;

namespace Crpg.Infrastructure.Metrics.Debug
{
    internal class DebugMetric : ICount, IHistogram, IGauge
    {
        private readonly string _name;
        private readonly ILogger _logger;

        public DebugMetric(string name, ILogger logger)
        {
            _name = name;
            _logger = logger;
        }

        public void Record(double value) => _logger.LogDebug($"{_name} record {value}");
        public void Increment(long delta) => _logger.LogDebug($"{_name} increment by {delta}");
        public void Decrement(long delta) => _logger.LogDebug($"{_name} decrement by {delta}");
        public void Dispose() { }
    }
}