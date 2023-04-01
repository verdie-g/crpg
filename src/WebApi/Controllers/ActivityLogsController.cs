using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.ActivityLogs.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.ActivityLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Route("activity-logs")]
public class ActivityLogsController : BaseController
{
    /// <summary>
    /// Get activity logs. The result is limited too 1000 logs.
    /// </summary>
    /// <query name="from">Start of the queried time period.</query>
    /// <query name="to">End of the queried time period.</query>
    /// <query name="userIds">User ids to filer the logs. Ignored if empty.</query>
    /// <query name="types">Log types to filer the logs. Ignored if empty.</query>
    /// <returns>The activity logs.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad Request.</response>
    [Authorize(Policy = ModeratorPolicy)]
    [HttpGet]
    public async Task<ActionResult<Result<IList<ActivityLogViewModel>>>> GetActivityLogs(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery(Name = "userId[]")] int[]? userIds,
        [FromQuery(Name = "type[]")] ActivityLogType[]? types)
    {
        return ResultToAction(await Mediator.Send(new GetActivityLogsQuery
        {
            From = from,
            To = to,
            UserIds = userIds ?? Array.Empty<int>(),
            Types = types ?? Array.Empty<ActivityLogType>(),
        }, CancellationToken.None));
    }
}
