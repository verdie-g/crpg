using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Crpg.Application.Common.Interfaces;

namespace Crpg.Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public abstract class BaseController : ControllerBase
    {
        private IMediator _mediator;
        private ICurrentUserService _currentUser;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
        protected ICurrentUserService CurrentUser => _currentUser ??= HttpContext.RequestServices.GetService<ICurrentUserService>();
    }
}