using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.ActivityLogs.Queries;
using Crpg.Application.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Route("activity-logs")]
public class ActivityLogsController : BaseController
{
    /// <summary>
    /// Get activity logs.
    /// </summary>
    /// <query name="count">The number of logs to return.</query>
    /// <query name="after">The id where to start the search.</query>
    /// <returns>The activity logs.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad Request.</response>
    [Authorize(Policy = ModeratorPolicy)]
    [HttpGet]
    public async Task<ActionResult<Result<IList<ActivityLogViewModel>>>> GetActivityLogs(
        [FromQuery] int count,
        [FromQuery] int? afterId)
    {
        return ResultToAction(await Mediator.Send(new GetActivityLogsQuery
        {
            Count = count,
            AfterId = afterId,
        }, CancellationToken.None));
    }
}
