using Crpg.Application.ActivityLogs.Commands;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Application.Restrictions.Commands;
using Crpg.Application.Restrictions.Models;
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
    public Task<ActionResult<Result<GameUserViewModel>>> GetUser(
        [FromQuery] Platform platform, [FromQuery] string platformUserId) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserCommand
        {
            Platform = platform,
            PlatformUserId = platformUserId,
        }));

    /// <summary>
    /// Get tournament user.
    /// </summary>
    [HttpGet("tournament-users")]
    public Task<ActionResult<Result<GameUserViewModel>>> GetTournamentUser(
        [FromQuery] Platform platform, [FromQuery] string platformUserId) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserTournamentCommand
        {
            Platform = platform,
            PlatformUserId = platformUserId,
        }));

    /// <summary>
    /// Give reward to users and break or repair items.
    /// </summary>
    [HttpPut("users")]
    public Task<ActionResult<Result<UpdateGameUsersResult>>> UpdateUsers([FromBody] UpdateGameUsersCommand cmd) =>
        ResultToActionAsync(Mediator.Send(cmd));

    /// <summary>
    /// Insert activity logs.
    /// </summary>
    /// <param name="activityLogs">The activity logs to insert.</param>
    /// <response code="200">Inserted.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPost("activity-logs")]
    public Task<ActionResult> InsertActivityLogs([FromBody] ActivityLogViewModel[] activityLogs)
    {
        return ResultToActionAsync(Mediator.Send(new CreateActivityLogsCommand
        {
            ActivityLogs = activityLogs,
        }, CancellationToken.None));
    }

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

    [HttpPost("restrictions")]
    public Task<ActionResult<Result<RestrictionViewModel>>> RestrictUser([FromBody] RestrictCommand req)
    {
        return ResultToActionAsync(Mediator.Send(req));
    }
}
