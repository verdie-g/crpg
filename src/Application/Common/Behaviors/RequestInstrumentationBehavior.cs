using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces.Tracing;
using Crpg.Common;
using Crpg.Common.Helpers;
using MediatR;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestInstrumentationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private const string SpanName = "request";
        private static readonly KeyValuePair<string, string>[] SpanTags =
        {
            new KeyValuePair<string, string>("name", StringHelper.PascalToSnakeCase(typeof(TRequest).Name))
        };

        private readonly RequestMetrics<TRequest> _metrics;
        private readonly ITracer _tracer;

        public RequestInstrumentationBehavior(RequestMetrics<TRequest> metrics, ITracer tracer)
        {
            _metrics = metrics;
            _tracer = tracer;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var span = _tracer.CreateSpan(SpanName, SpanTags);
            var sw = ValueStopwatch.StartNew();
            try
            {
                var res = await next();
                _metrics.StatusOk.Increment();
                return res;
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case ValidationException _:
                        _metrics.StatusErrorBadRequest.Increment();
                        break;
                    case BadRequestException _:
                        _metrics.StatusErrorBadRequest.Increment();
                        break;
                    case NotFoundException _:
                        _metrics.StatusErrorNotFound.Increment();
                        break;
                    case ConflictException _:
                        _metrics.StatusErrorConflict.Increment();
                        break;
                    default:
                        _metrics.StatusErrorUnknown.Increment();
                        break;
                }

                throw;
            }
            finally
            {
                _metrics.ResponseTime.Record(sw.Elapsed.TotalMilliseconds);
                span.Dispose();
            }
        }
    }
}
