using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces.Metrics;
using Crpg.Common;
using Crpg.Common.Helpers;
using MediatR;

namespace Crpg.Application.Common.Behaviors
{
    public class RequestMetricBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private static ICount? _statusOk;
        private static ICount? _statusErrorBadRequest;
        private static ICount? _statusErrorNotFound;
        private static ICount? _statusErrorForbidden;
        private static ICount? _statusErrorUnknown;
        private static IHistogram? _responseTime;

        public RequestMetricBehavior(IMetricsFactory metricsFactory)
        {
            if (_responseTime == null)
            {
                CreateMetrics(metricsFactory);
            }
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var sw = ValueStopwatch.StartNew();
            try
            {
                var res = await next();
                _statusOk!.Increment();
                return res;
            }
            catch (ValidationException)
            {
                _statusErrorBadRequest!.Increment();
                throw;
            }
            catch (BadRequestException)
            {
                _statusErrorBadRequest!.Increment();
                throw;
            }
            catch (NotFoundException)
            {
                _statusErrorNotFound!.Increment();
                throw;
            }
            catch (ForbiddenException)
            {
                _statusErrorForbidden!.Increment();
                throw;
            }
            catch (Exception)
            {
                _statusErrorUnknown!.Increment();
                throw;
            }
            finally
            {
                _responseTime!.Record(sw.Elapsed.TotalMilliseconds);
            }
        }

        private void CreateMetrics(IMetricsFactory metricsFactory)
        {
            string requestNameTag = "name:" + StringHelper.PascalToSnakeCase(typeof(TRequest).Name);
            const string metricStatus = "requests.status";
            const string metricResponseTime = "requests.response_time";
            const string statusKeyPrefix = "status:";

            // another thread might be in this method so cas each one and dispose if race is lost
            ReplaceMetric(ref _statusOk,
                metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "ok" }));
            ReplaceMetric(ref _statusErrorBadRequest,
                metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "bad_request" }));
            ReplaceMetric(ref _statusErrorNotFound,
                metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "not_found" }));
            ReplaceMetric(ref _statusErrorForbidden,
                metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "forbidden" }));
            ReplaceMetric(ref _statusErrorUnknown,
                metricsFactory.CreateCount(metricStatus, new[] { requestNameTag, statusKeyPrefix + "unknown" }));
            ReplaceMetric(ref _responseTime,
                metricsFactory.CreateHistogram(metricResponseTime, tags: new[] { requestNameTag }));
        }

        private void ReplaceMetric<TMetric>(ref TMetric? toReplace, TMetric metric) where TMetric : class, IMetric
        {
            if (Interlocked.CompareExchange(ref toReplace, metric, null) != null)
            {
                metric.Dispose();
            }
        }
    }
}