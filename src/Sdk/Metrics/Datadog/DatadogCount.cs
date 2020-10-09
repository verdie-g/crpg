using Crpg.Sdk.Abstractions.Metrics;
using DatadogStatsD.Metrics;

namespace Crpg.Sdk.Metrics.Datadog
{
    internal class DatadogCount : ICount
    {
        private readonly Count _underlyingCount;

        public DatadogCount(Count count) => _underlyingCount = count;
        public void Increment(long delta) => _underlyingCount.Increment(delta);
        public void Decrement(long delta) => _underlyingCount.Decrement(delta);
        public void Dispose() => _underlyingCount.Dispose();
    }
}
