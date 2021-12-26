using Crpg.Application.Bans.Commands;
using Crpg.Application.Bans.Models;
using Crpg.Application.Bans.Queries;
using Crpg.Application.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = ModeratorPolicy)]
public class BansController : BaseController
{
    /// <summary>
    /// Gets all bans.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [ResponseCache(Duration = 60 * 60 * 1)] // 1 hour
    public Task<ActionResult<Result<IList<BanViewModel>>>> GetBans() =>
        ResultToActionAsync(Mediator.Send(new GetBansQuery()));

    /// <summary>
    /// Bans an user. If a ban already exists for the user, it is overriden. Use a duration of 0 to unban.
    /// </summary>
    /// <param name="req">Ban info.</param>
    /// <returns>The ban object.</returns>
    /// <response code="201">Banned.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="404">User was not found.</response>
    [HttpPost]
    public Task<ActionResult<Result<BanViewModel>>> BanUser([FromBody] BanCommand req)
    {
        req = req with { BannedByUserId = CurrentUser.UserId };
        return ResultToCreatedAtActionAsync(nameof(GetBans), null, b => new { id = b.Id },
            Mediator.Send(req));
    }
}
