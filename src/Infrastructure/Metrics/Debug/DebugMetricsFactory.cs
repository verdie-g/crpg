using System;
using System.Collections.Generic;
using Crpg.Application.Common.Interfaces.Metrics;
using Microsoft.Extensions.Logging;

namespace Crpg.Infrastructure.Metrics.Debug
{
    internal class DebugMetricsFactory : IMetricsFactory
    {
        private readonly ILogger _logger;

        public DebugMetricsFactory(ILogger<DebugMetricsFactory> logger)
        {
            _logger = logger;
        }

        public ICount CreateCount(string metricName, IList<string>? tags = null) => new DebugMetric(metricName, _logger);
        public IHistogram CreateHistogram(string metricName, double sampleRate = 1, IList<string>? tags = null) => new DebugMetric(metricName, _logger);
        public IGauge CreateGauge(string metricName, Func<double> evaluator, IList<string>? tags = null) => new DebugMetric(metricName, _logger);
        public void Dispose() { }
    }
}