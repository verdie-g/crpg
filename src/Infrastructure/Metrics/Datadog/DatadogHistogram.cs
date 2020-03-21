using Crpg.Application.Common.Interfaces.Metrics;
using DatadogStatsD.Metrics;

namespace Crpg.Infrastructure.Metrics.Datadog
{
    internal class DatadogHistogram : IHistogram
    {
        private readonly Histogram _underlyingHistogram;

        public DatadogHistogram(Histogram histogram) => _underlyingHistogram = histogram;
        public void Record(double value) => _underlyingHistogram.Record(value);
        public void Dispose() => _underlyingHistogram.Dispose();
    }
}