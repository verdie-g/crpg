using Microsoft.AspNetCore.Mvc;
using Trpg.Application.Users.Queries;

namespace Trpg.WebApi.Controllers
{
    [ApiController]
    public class UsersController : BaseController
    {
        [HttpGet("self")]
        public IActionResult GetSelfUser()
        {
            return Ok(Mediator.Send(new GetUserQuery { UserId = CurrentUser.UserId.Value }));
        }
    }
}
