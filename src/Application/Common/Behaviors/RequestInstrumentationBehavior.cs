using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Results;
using Crpg.Common;
using Crpg.Common.Helpers;
using Crpg.Sdk.Abstractions.Tracing;
using MediatR;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestInstrumentationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private static readonly string SpanName = "request." + StringHelper.PascalToSnakeCase(typeof(TRequest).Name);

        private readonly RequestMetrics<TRequest> _metrics;
        private readonly ITracer _tracer;

        public RequestInstrumentationBehavior(RequestMetrics<TRequest> metrics, ITracer tracer)
        {
            _metrics = metrics;
            _tracer = tracer;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var span = _tracer.CreateSpan(SpanName);
            var sw = ValueStopwatch.StartNew();
            try
            {
                var response = await next();
                if (response is Result result && result.Errors != null && result.Errors.Count != 0)
                {
                    _metrics.StatusErrorBadRequest.Increment();
                }
                else
                {
                    _metrics.StatusOk.Increment();
                }

                return response;
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case ConflictException _:
                        _metrics.StatusErrorConflict.Increment();
                        break;
                    default:
                        _metrics.StatusErrorUnknown.Increment();
                        break;
                }

                throw; // Will be caught in CustomExceptionHandlerMiddleware.
            }
            finally
            {
                _metrics.ResponseTime.Record(sw.Elapsed.TotalMilliseconds);
                span.Dispose();
            }
        }
    }
}
