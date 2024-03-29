using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
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
    [ResponseCache(Duration = 1 * 60 * 1)] // 1 minutes
    public Task<ActionResult<Result<IList<CharacterPublicViewModel>>>> GetLeaderboard(
        [FromQuery] Region? region,
        [FromQuery] CharacterClass? characterClass)
    {
        return ResultToActionAsync(Mediator.Send(new GetLeaderboardQuery
        {
            Region = region,
            CharacterClass = characterClass,
        }));
    }
}
