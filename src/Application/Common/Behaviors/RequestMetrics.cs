using Crpg.Application.Common.Interfaces.Metrics;
using Crpg.Common.Helpers;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestMetrics<TRequest>
    {
        public ICount StatusOk { get; }
        public ICount StatusErrorBadRequest { get; }
        public ICount StatusErrorNotFound { get; }
        public ICount StatusErrorForbidden { get; }
        public ICount StatusErrorUnknown { get; }
        public IHistogram ResponseTime { get; }

        public RequestMetrics(IMetricsFactory metricsFactory)
        {
            string requestNameTag = "name:" + StringHelper.PascalToSnakeCase(typeof(TRequest).Name);
            const string metricStatus = "requests.status";
            const string metricResponseTime = "requests.response_time";
            const string statusKeyPrefix = "status:";

            StatusOk = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "ok" });
            StatusErrorBadRequest = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "bad_request" });
            StatusErrorNotFound = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "not_found" });
            StatusErrorForbidden = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "forbidden" });
            StatusErrorUnknown = metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "unknown" });
            ResponseTime = metricsFactory.CreateHistogram(metricResponseTime, tags: new[] { requestNameTag });
        }
    }
}