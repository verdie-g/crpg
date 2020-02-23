using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Trpg.Application.Common.Exceptions;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Trpg.Persistence.Exceptions;

namespace Trpg.Web.Common
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
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e.Failures));
            }
            catch (BadRequestException e)
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e.Message));
            }
            catch (NotFoundException e)
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e.Message));
            }
            catch (ForbiddenException e)
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e.Message));
            }
            catch (ConflictException)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Conflict;
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