using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Common;
using MediatR;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestMetricBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly RequestMetrics<TRequest> _metrics;

        public RequestMetricBehavior(RequestMetrics<TRequest> metrics)
        {
            _metrics = metrics;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var sw = ValueStopwatch.StartNew();
            try
            {
                var res = await next();
                _metrics.StatusOk.Increment();
                return res;
            }
            catch (ValidationException)
            {
                _metrics.StatusErrorBadRequest.Increment();
                throw;
            }
            catch (BadRequestException)
            {
                _metrics.StatusErrorBadRequest.Increment();
                throw;
            }
            catch (NotFoundException)
            {
                _metrics.StatusErrorNotFound.Increment();
                throw;
            }
            catch (ConflictException)
            {
                _metrics.StatusErrorConflict.Increment();
                throw;
            }
            catch (Exception)
            {
                _metrics.StatusErrorUnknown.Increment();
                throw;
            }
            finally
            {
                _metrics.ResponseTime.Record(sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}