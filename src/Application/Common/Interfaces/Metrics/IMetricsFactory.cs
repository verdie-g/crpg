using System;
using System.Collections.Generic;

namespace Crpg.Application.Common.Interfaces.Metrics
{
    public interface IMetricsFactory : IDisposable
    {
        ICount CreateCount(string metricName, IList<string>? tags = null);
        IHistogram CreateHistogram(string metricName, double sampleRate = 1.0, IList<string>? tags = null);
        IGauge CreateGauge(string metricName, Func<double> evaluator, IList<string>? tags = null);
    }
}