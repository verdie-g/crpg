using Crpg.Application.Common.Interfaces.Metrics;
using DatadogStatsD.Metrics;

namespace Crpg.Infrastructure.Metrics.Datadog
{
    internal class DatadogGauge : IGauge
    {
        private readonly Gauge _underlyingGauge;

        public DatadogGauge(Gauge gauge) => _underlyingGauge = gauge;
        public void Dispose() => _underlyingGauge.Dispose();
    }
}