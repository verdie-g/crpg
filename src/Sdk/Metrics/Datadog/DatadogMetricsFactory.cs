using Crpg.Sdk.Abstractions.Metrics;
using DatadogStatsD;

namespace Crpg.Sdk.Metrics.Datadog;

internal class DatadogMetricsFactory : IMetricsFactory
{
    private readonly DogStatsD _dogStatsD;

    public DatadogMetricsFactory(DogStatsD dogStatsD) => _dogStatsD = dogStatsD;

    public ICount CreateCount(string metricName, IList<KeyValuePair<string, string>>? tags = null) =>
        new DatadogCount(_dogStatsD.CreateCount(metricName, tags));

    public IHistogram CreateHistogram(string metricName, double sampleRate = 1, IList<KeyValuePair<string, string>>? tags = null) =>
        new DatadogHistogram(_dogStatsD.CreateHistogram(metricName, sampleRate, tags));

    public IGauge CreateGauge(string metricName, Func<double> evaluator, IList<KeyValuePair<string, string>>? tags = null) =>
        new DatadogGauge(_dogStatsD.CreateGauge(metricName, evaluator, tags));

    public ValueTask DisposeAsync() => _dogStatsD.DisposeAsync();
}
