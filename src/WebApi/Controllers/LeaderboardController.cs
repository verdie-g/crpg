using System.Net;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Crpg.Application.Limitations.Models;
using Crpg.Application.Limitations.Queries;
using Crpg.Application.Restrictions.Models;
using Crpg.Application.Restrictions.Queries;
using Crpg.Application.Users.Commands;
using Crpg.Application.Users.Models;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class LeaderboardController : BaseController
{
    /// <summary>
    /// Get top character competitive ratings.
    /// </summary>
    /// <returns>The top character competitive ratings.</returns>
    /// <response code="200">Ok.</response>
    [HttpGet("leaderboard")]
    public Task<ActionResult<Result<IList<CharacterViewModel>>>> GetLeaderboard()
    {
        return ResultToActionAsync(Mediator.Send(new GetLeaderboardQuery()));
    }
}
