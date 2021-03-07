using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Commands;
using Crpg.Application.Strategus.Models;
using Crpg.Application.Strategus.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class StrategusController : BaseController
    {
        /// <summary>
        /// Register user to strategus.
        /// </summary>
        /// <returns>The new strategus user.</returns>
        /// <response code="200">Registered.</response>
        /// <response code="400">Already registered.</response>
        [HttpPost("users")]
        public Task<ActionResult<Result<StrategusUserViewModel>>> RegisterStrategusUser([FromBody] CreateStrategusUserCommand req)
        {
            req.UserId = CurrentUser.UserId;
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Get strategus settlements.
        /// </summary>
        [HttpGet("settlements")]
        public Task<ActionResult<Result<IList<StrategusSettlementViewModel>>>> GetUser()
            => ResultToActionAsync(Mediator.Send(new GetStrategusSettlementsQuery()));
    }
}
