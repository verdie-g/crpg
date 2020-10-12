using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Crpg.WebApi.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApplicationEnvironment _appEnv;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, IApplicationEnvironment appEnv)
        {
            _next = next;
            _appEnv = appEnv;
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
                            },
                        };
                        break;
                    default:
                        httpStatus = HttpStatusCode.InternalServerError;
                        errors = new[]
                        {
                            new Error(ErrorType.InternalError, ErrorCode.InternalError)
                            {
                                Title = "Unknown error. Contact an administrator",
                            },
                        };
                        break;
                }

                // Only include exception in the response during development.
                if (_appEnv.Environment == HostingEnvironment.Development)
                {
                    foreach (var error in errors)
                    {
                        error.StackTrace = e.StackTrace;
                    }
                }

                var result = new Result<object>(errors);

                ctx.Response.StatusCode = (int)httpStatus;
                ctx.Response.ContentType = MediaTypeNames.Application.Json;
                await ctx.Response.WriteAsync(JsonConvert.SerializeObject(result));
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
