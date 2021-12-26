using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Results;
using Crpg.Common;
using Crpg.Common.Helpers;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Tracing;
using MediatR;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Behaviors;

internal class RequestInstrumentationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private const string OperationName = "request";
    private static readonly ILogger Logger = LoggerFactory.CreateLogger(typeof(RequestInstrumentationBehavior<,>));
    private static readonly string ResourceName = StringHelper.PascalToSnakeCase(typeof(TRequest).Name);

    static RequestInstrumentationBehavior()
    {
        Debug.Assert(typeof(Result).IsAssignableFrom(typeof(TResponse)),
            $"Request {typeof(TRequest).Name} should return a {nameof(Result)} type");
    }

    private readonly RequestMetrics<TRequest> _metrics;
    private readonly ITracer _tracer;
    private readonly IApplicationEnvironment _appEnv;

    public RequestInstrumentationBehavior(RequestMetrics<TRequest> metrics, ITracer tracer, IApplicationEnvironment appEnv)
    {
        _metrics = metrics;
        _tracer = tracer;
        _appEnv = appEnv;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var span = _tracer.CreateSpan(OperationName, ResourceName);
        var sw = ValueStopwatch.StartNew();
        try
        {
            var response = await next();
            if (response is Result result && result.Errors != null && result.Errors.Count != 0)
            {
                switch (result.Errors[0].Type)
                {
                    case ErrorType.Validation:
                        _metrics.StatusErrorBadRequest.Increment();
                        break;
                    case ErrorType.NotFound:
                        _metrics.StatusErrorNotFound.Increment();
                        break;
                    default:
                        _metrics.StatusErrorUnknown.Increment();
                        break;
                }
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

                    Logger.Log(LogLevel.Error, e, "Conflict");
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

                    Logger.Log(LogLevel.Error, e, "Unknown error");
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
