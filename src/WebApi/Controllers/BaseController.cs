using System.Net.Mime;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
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
        private IMediator? _mediator;
        private ICurrentUserService? _currentUser;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
        protected ICurrentUserService CurrentUser => _currentUser ??= HttpContext.RequestServices.GetService<ICurrentUserService>();

        protected void CheckIsSelfUserId(int userId)
        {
            if (userId != CurrentUser.UserId)
            {
                throw new ForbiddenException(nameof(Domain.Entities.User), userId);
            }
        }
    }
}