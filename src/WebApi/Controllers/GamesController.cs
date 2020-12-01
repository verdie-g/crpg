using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = GamePolicy)]
    public class GamesController : BaseController
    {
        /// <summary>
        /// Get or create user.
        /// </summary>
        [HttpGet("users")]
        public Task<ActionResult<Result<GameUser>>> GetUser([FromQuery] Platform platform,
            [FromQuery] string platformUserId, [FromQuery] string userName) =>
            ResultToActionAsync(Mediator.Send(new GetGameUserCommand
            {
                Platform = platform,
                PlatformUserId = platformUserId,
                UserName = userName,
            }));

        /// <summary>
        /// Give reward to users and break or repair items.
        /// </summary>
        [HttpPut("users")]
        public Task<ActionResult<Result<UpdateGameUsersResult>>> UpdateUsers([FromBody] UpdateGameUsersCommand cmd) =>
            ResultToActionAsync(Mediator.Send(cmd));
    }
}
