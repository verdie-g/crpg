using Crpg.Sdk.Abstractions.Metrics;
using DatadogStatsD.Metrics;

namespace Crpg.Sdk.Metrics.Datadog
{
    internal class DatadogHistogram : IHistogram
    {
        private readonly Histogram _underlyingHistogram;

        public DatadogHistogram(Histogram histogram) => _underlyingHistogram = histogram;
        public void Record(double value) => _underlyingHistogram.Sample(value);
        public void Dispose() => _underlyingHistogram.Dispose();
    }
}
