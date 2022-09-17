using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Commands;
using Crpg.Application.Restrictions.Models;
using Crpg.Application.Restrictions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = ModeratorPolicy)]
public class RestrictionsController : BaseController
{
    /// <summary>
    /// Get all restrictions.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [ResponseCache(Duration = 60 * 60 * 1)] // 1 hour
    public Task<ActionResult<Result<IList<RestrictionViewModel>>>> GetRestrictions() =>
        ResultToActionAsync(Mediator.Send(new GetRestrictionsQuery()));

    /// <summary>
    /// Restrict a user. If a restriction of the same type already exists for the user, it is overriden. Use a duration
    /// of 0 to un-restrict.
    /// </summary>
    /// <param name="req">Restriction info.</param>
    /// <returns>The restriction object.</returns>
    /// <response code="201">Restricted.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="404">User was not found.</response>
    [HttpPost]
    public Task<ActionResult<Result<RestrictionViewModel>>> RestrictUser([FromBody] RestrictCommand req)
    {
        req = req with { RestrictedByUserId = CurrentUser.User!.Id };
        return ResultToCreatedAtActionAsync(nameof(GetRestrictions), null, r => new { id = r.Id },
            Mediator.Send(req));
    }
}
