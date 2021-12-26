using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
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

    /// <summary>
    /// Gets all items including loomed and broken ones.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet("items")]
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItems() =>
        ResultToActionAsync(Mediator.Send(new GetItemsQuery { BaseItems = false }));
}
