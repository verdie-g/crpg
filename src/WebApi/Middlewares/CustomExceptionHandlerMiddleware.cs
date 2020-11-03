using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crpg.WebApi.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };

        private readonly RequestDelegate _next;
        private readonly IApplicationEnvironment _appEnv;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, IApplicationEnvironment appEnv,
            ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _appEnv = appEnv;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception e)
            {
                HttpStatusCode httpStatus;
                Error[] errors;

                switch (e)
                {
                    case ValidationException ve:
                        httpStatus = HttpStatusCode.BadRequest;
                        errors = ve.Errors.Select(e => new Error(ErrorType.Validation, ErrorCode.InvalidField)
                        {
                            Title = "Invalid field",
                            Detail = e.ErrorMessage,
                            Source = new ErrorSource { Parameter = e.PropertyName },
                        }).ToArray();
                        break;
                    case ConflictException _:
                        httpStatus = HttpStatusCode.Conflict;
                        errors = new[]
                        {
                            new Error(ErrorType.InternalError, ErrorCode.Conflict)
                            {
                                Title = "Request conflicted with another concurring one",
                                Detail = _appEnv.Environment == HostingEnvironment.Development ? e.Message : null,
                                StackTrace = _appEnv.Environment == HostingEnvironment.Development ? e.StackTrace : null,
                            },
                        };

                        _logger.Log(LogLevel.Error, e, "Conflict");
                        break;
                    default:
                        httpStatus = HttpStatusCode.InternalServerError;
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

                var result = new Result<object>(errors);

                ctx.Response.StatusCode = (int)httpStatus;
                ctx.Response.ContentType = MediaTypeNames.Application.Json;
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(result, SerializerOptions));
            }
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
