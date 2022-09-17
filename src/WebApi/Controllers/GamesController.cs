using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = GamePolicy)]
public class GamesController : BaseController
{
    /// <summary>
    /// Get or create user.
    /// </summary>
    [HttpGet("users")]
    public Task<ActionResult<Result<GameUserViewModel>>> GetUser([FromQuery] Platform platform,
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

    // TODO: this endpoint is a duplicate of /clans/{id} because I could not find a good way for an endpoint to allow
    // both the user and game policies.

    /// <summary>
    /// Gets a clan from its id.
    /// </summary>
    /// <response code="200">Ok.</response>
    /// <response code="404">Clan was not found.</response>
    [HttpGet("clans/{id}")]
    public Task<ActionResult<Result<ClanViewModel>>> GetClan([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new GetClanQuery { ClanId = id }));
}
