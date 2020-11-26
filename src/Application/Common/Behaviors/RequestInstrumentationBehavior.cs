using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Results;
using Crpg.Common;
using Crpg.Common.Helpers;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Tracing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Common.Behaviors
{
    internal class RequestInstrumentationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : class
    {
        private static readonly string SpanName = "request." + StringHelper.PascalToSnakeCase(typeof(TRequest).Name);

        static RequestInstrumentationBehavior()
        {
            // All requests should return a Result object to have a consistent API.
            if (!typeof(Result).IsAssignableFrom(typeof(TResponse)))
            {
                throw new Exception($"Request {typeof(TRequest).Name} should return a {nameof(Result)} type");
            }
        }

        private readonly RequestMetrics<TRequest> _metrics;
        private readonly ITracer _tracer;
        private readonly IApplicationEnvironment _appEnv;
        private readonly ILogger<RequestInstrumentationBehavior<TRequest, TResponse>> _logger;

        public RequestInstrumentationBehavior(RequestMetrics<TRequest> metrics, ITracer tracer,
            IApplicationEnvironment appEnv, ILogger<RequestInstrumentationBehavior<TRequest, TResponse>> logger)
        {
            _metrics = metrics;
            _tracer = tracer;
            _appEnv = appEnv;
            _logger = logger;
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
                span.SetException(e);

                Error[] errors;
                switch (e)
                {
                    case ValidationException ve:
                        _metrics.StatusErrorBadRequest.Increment();
                        errors = ve.Errors.Select(ve => new Error(ErrorType.Validation, ErrorCode.InvalidField)
                        {
                            Title = "Invalid field",
                            Detail = ve.ErrorMessage,
                            Source = new ErrorSource { Parameter = ve.PropertyName },
                        }).ToArray();
                        break;
                    case ConflictException _:
                        _metrics.StatusErrorConflict.Increment();
                        errors = new[]
                        {
                            new Error(ErrorType.Conflict, ErrorCode.Conflict)
                            {
                                Title = "Request conflicted with another concurring one",
                                Detail = _appEnv.Environment == HostingEnvironment.Development ? e.Message : null,
                                StackTrace = _appEnv.Environment == HostingEnvironment.Development ? e.StackTrace : null,
                            },
                        };

                        _logger.Log(LogLevel.Error, e, "Conflict");
                        break;
                    default:
                        _metrics.StatusErrorUnknown.Increment();
                        errors = new[]
                        {
                            new Error(ErrorType.InternalError, ErrorCode.InternalError)
                            {
                                Title = "Unknown error. Contact an administrator",
                                Detail = _appEnv.Environment == HostingEnvironment.Development ? e.Message : null,
                                StackTrace = _appEnv.Environment == HostingEnvironment.Development ? e.StackTrace : null,
                            },
                        };

                        _logger.Log(LogLevel.Error, e, "Unknown error");
                        break;
                }

                return Unsafe.As<TResponse>(new Result(errors));
            }
            finally
            {
                _metrics.ResponseTime.Record(sw.Elapsed.TotalMilliseconds);
                span.Dispose();
            }
        }
    }
}
