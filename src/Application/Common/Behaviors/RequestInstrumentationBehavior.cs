using System.Diagnostics;
using System.Runtime.CompilerServices;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using Crpg.Common;
using Crpg.Sdk.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Behaviors;

internal class RequestInstrumentationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger(typeof(RequestInstrumentationBehavior<,>));
    private static readonly RequestInstrumentation Instrumentation = new(typeof(TRequest));

    static RequestInstrumentationBehavior()
    {
        Debug.Assert(typeof(Result).IsAssignableFrom(typeof(TResponse)),
            $"Request {typeof(TRequest).Name} should return a {nameof(Result)} type");
    }

    private readonly IApplicationEnvironment _appEnv;
    private readonly ICurrentUserService _currentUser;

    public RequestInstrumentationBehavior(IApplicationEnvironment appEnv, ICurrentUserService currentUser)
    {
        _appEnv = appEnv;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var span = Instrumentation.StartRequestSpan();
        IDisposable? loggingScope = null;
        if (_currentUser.User != null)
        {
            KeyValuePair<string, object?>[] tags =
            {
                new("enduser.id", _currentUser.User.Id),
                new("enduser.role", _currentUser.User.Role.ToString()),
            };
            loggingScope = Logger.BeginScope(tags);
            if (span != null)
            {
                foreach (var tag in tags)
                {
                    span.SetTag(tag.Key, tag.Value);
                }
            }
        }

        var sw = ValueStopwatch.StartNew();
        try
        {
            var response = await next();
            if (response is Result result && result.Errors != null && result.Errors.Count != 0)
            {
                switch (result.Errors[0].Type)
                {
                    case ErrorType.Validation:
                        Instrumentation.IncrementBadRequest();
                        break;
                    case ErrorType.NotFound:
                        Instrumentation.IncrementNotFound();
                        break;
                    default:
                        Instrumentation.IncrementUnknown();
                        break;
                }
            }
            else
            {
                Instrumentation.IncrementOk();
            }

            span?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception e)
        {
            span?.SetStatus(ActivityStatusCode.Error, e.Message);

            Error[] errors;
            switch (e)
            {
                case ConflictException _:
                    Instrumentation.IncrementConflict();
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
                    Instrumentation.IncrementUnknown();
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
            Instrumentation.RecordResponseTime(sw.Elapsed.TotalMilliseconds);
            loggingScope?.Dispose();
            span?.Dispose();
        }
    }
}
