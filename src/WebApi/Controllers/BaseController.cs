using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public abstract class BaseController : ControllerBase
    {
        // Constants used by Authorize attributes in controllers. Their values are the policies defined in Startup.
        protected const string UserPolicy = "User";
        protected const string ModeratorPolicy = "Moderator";
        protected const string AdminPolicy = "Admin";
        protected const string GamePolicy = "Game";

        private IMediator? _mediator;
        private ICurrentUserService? _currentUser;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected ICurrentUserService CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        protected ActionResult<Result<TData>> ResultToAction<TData>(Result<TData> result)
            where TData : class
        {
            return result.Errors == null || result.Errors.Count == 0 ? Ok(result) : FirstErrorToAction(result);
        }

        protected ActionResult<Result<TData>> ResultToCreatedAtAction<TData>(
            string actionName, string? controllerName, Func<TData, object>? getRouteValues, Result<TData> result)
            where TData : class
        {
            return result.Errors == null || result.Errors.Count == 0
                ? CreatedAtAction(actionName, controllerName, getRouteValues?.Invoke(result.Data!), result)
                : FirstErrorToAction(result);
        }

        protected ActionResult ResultToAction(Result result)
        {
            return result.Errors == null || result.Errors.Count == 0 ? NoContent() : FirstErrorToAction(result);
        }

        protected async Task<ActionResult<Result<TData>>> ResultToActionAsync<TData>(Task<Result<TData>> resultTask)
            where TData : class
        {
            var result = await resultTask;
            return ResultToAction(result);
        }

        protected async Task<ActionResult<Result<TData>>> ResultToCreatedAtActionAsync<TData>(
            string actionName, string? controllerName, Func<TData, object>? getRouteValues, Task<Result<TData>> resultTask)
            where TData : class
        {
            var result = await resultTask;
            return ResultToCreatedAtAction(actionName, controllerName, getRouteValues, result);
        }

        protected async Task<ActionResult> ResultToActionAsync(Task<Result> resultTask)
        {
            var result = await resultTask;
            return ResultToAction(result);
        }

        private ActionResult FirstErrorToAction(Result result)
        {
            string traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            foreach (var error in result.Errors!)
            {
                error.TraceId = traceId;
            }

            Error firstError = result.Errors.First();
            return firstError.Type switch
            {
                ErrorType.Validation => BadRequest(result),
                ErrorType.NotFound => NotFound(result),
                ErrorType.Conflict => Conflict(result),
                ErrorType.Forbidden => StatusCode((int)HttpStatusCode.Forbidden, result),
                _ => StatusCode((int)HttpStatusCode.InternalServerError, result),
            };
        }
    }
}
