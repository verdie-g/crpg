using Crpg.Sdk.Abstractions.Metrics;

namespace Crpg.Sdk.Metrics.Debug;

internal class DebugMetricsFactory : IMetricsFactory
{
    public ICount CreateCount(string metricName, IList<KeyValuePair<string, string>>? tags = null) =>
        new DebugMetric(metricName, tags);
    public IHistogram CreateHistogram(string metricName, double sampleRate = 1, IList<KeyValuePair<string, string>>? tags = null) =>
        new DebugMetric(metricName, tags);
    public IGauge CreateGauge(string metricName, Func<double> evaluator, IList<KeyValuePair<string, string>>? tags = null) =>
        new DebugMetric(metricName, tags);
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
