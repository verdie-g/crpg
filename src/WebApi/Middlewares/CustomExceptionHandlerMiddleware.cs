using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Crpg.WebApi.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException e)
            {
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, e.Message, e.Failures);
            }
            catch (BadRequestException e)
            {
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, e.Message);
            }
            catch (NotFoundException e)
            {
                await WriteErrorResponse(context, HttpStatusCode.NotFound, e.Message);
            }
            catch (ConflictException e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, e.Message);
            }
        }

        private Task WriteErrorResponse(HttpContext ctx, HttpStatusCode status, string error,
            IDictionary<string, string[]>? details = null)
        {
            ctx.Response.ContentType = MediaTypeNames.Application.Json;
            ctx.Response.StatusCode = (int)status;
            return ctx.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse
            {
                Error = error,
                Details = details,
            }));
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