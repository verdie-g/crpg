using Crpg.Common.Helpers;
using Crpg.Sdk.Abstractions.Metrics;

namespace Crpg.Application.Common.Behaviors;

internal class RequestMetrics<TRequest>
{
    public ICount StatusOk { get; }
    public ICount StatusErrorBadRequest { get; }
    public ICount StatusErrorNotFound { get; }
    public ICount StatusErrorConflict { get; }
    public ICount StatusErrorUnknown { get; }
    public IHistogram ResponseTime { get; }

    public RequestMetrics(IMetricsFactory metricsFactory)
    {
        var requestNameTag = KeyValuePair.Create("name", StringHelper.PascalToSnakeCase(typeof(TRequest).Name));
        const string metricStatus = "requests.status";
        const string metricResponseTime = "requests.response_time";
        const string statusKey = "status";

        StatusOk = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, KeyValuePair.Create(statusKey, "ok") });
        StatusErrorBadRequest = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, KeyValuePair.Create(statusKey, "bad_request") });
        StatusErrorNotFound = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, KeyValuePair.Create(statusKey, "not_found") });
        StatusErrorConflict = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, KeyValuePair.Create(statusKey, "conflict") });
        StatusErrorUnknown = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, KeyValuePair.Create(statusKey, "unknown") });
        ResponseTime = metricsFactory.CreateHistogram(metricResponseTime, tags: new[] { requestNameTag });
    }
}
