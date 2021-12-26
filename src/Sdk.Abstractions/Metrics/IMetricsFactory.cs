namespace Crpg.Sdk.Abstractions.Metrics;

/// <summary>
/// Responsible for creating metrics to send to Datadog.
/// </summary>
public interface IMetricsFactory : IAsyncDisposable
{
    ICount CreateCount(string metricName, IList<KeyValuePair<string, string>>? tags = null);
    IHistogram CreateHistogram(string metricName, double sampleRate = 1.0, IList<KeyValuePair<string, string>>? tags = null);
    IGauge CreateGauge(string metricName, Func<double> evaluator, IList<KeyValuePair<string, string>>? tags = null);
}
