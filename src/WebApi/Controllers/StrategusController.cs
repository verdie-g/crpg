using System.Collections.Generic;
using System.Net;
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
        /// Get an update of strategus for the current user.
        /// </summary>
        /// <returns>Current strategus user, visible users and settlements, etc.</returns>
        /// <response code="200">Ok.</response>
        /// <response code="400">User was not registered to strategus.</response>
        [HttpGet("users/self")]
        public Task<ActionResult<Result<StrategusUpdate>>> GetStrategusUser()
        {
            return ResultToActionAsync(Mediator.Send(new GetStrategusUpdateQuery
            {
                UserId = CurrentUser.UserId,
            }));
        }

        /// <summary>
        /// Register user to strategus.
        /// </summary>
        /// <returns>The new strategus user.</returns>
        /// <response code="201">Registered.</response>
        /// <response code="400">Already registered.</response>
        [HttpPost("users")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public Task<ActionResult<Result<StrategusUserViewModel>>> RegisterStrategusUser([FromBody] CreateStrategusUserCommand req)
        {
            req.UserId = CurrentUser.UserId;
            return ResultToCreatedAtActionAsync(nameof(GetStrategusUser), null, null, Mediator.Send(req));
        }

        /// <summary>
        /// Update strategus user moves.
        /// </summary>
        /// <returns>The updated strategus user.</returns>
        /// <response code="200">Updated.</response>
        [HttpPut("users/self/moves")]
        public Task<ActionResult<Result<StrategusUserViewModel>>> UpdateStrategusUserMovement([FromBody] UpdateStrategusUserMovementCommand req)
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
