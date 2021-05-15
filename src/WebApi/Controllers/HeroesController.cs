using System.Net;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Heroes.Commands;
using Crpg.Application.Heroes.Models;
using Crpg.Application.Heroes.Queries;
using Crpg.Application.Strategus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class HeroesController : BaseController
    {
        /// <summary>
        /// Get an update of strategus for the current user.
        /// </summary>
        /// <returns>Current strategus hero, visible heroes and settlements, etc.</returns>
        /// <response code="200">Ok.</response>
        /// <response code="404">User was not registered to strategus.</response>
        [HttpGet("self/update")]
        public Task<ActionResult<Result<StrategusUpdate>>> GetStrategusUpdate()
        {
            return ResultToActionAsync(Mediator.Send(new GetStrategusUpdateQuery
            {
                HeroId = CurrentUser.UserId,
            }));
        }

        /// <summary>
        /// Register user to strategus.
        /// </summary>
        /// <returns>The new strategus hero.</returns>
        /// <response code="201">Registered.</response>
        /// <response code="400">Already registered.</response>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public Task<ActionResult<Result<HeroViewModel>>> RegisterHero([FromBody] CreateHeroCommand req)
        {
            req.UserId = CurrentUser.UserId;
            return ResultToCreatedAtActionAsync(nameof(GetStrategusUpdate), null, null, Mediator.Send(req));
        }

        /// <summary>
        /// Update strategus hero status.
        /// </summary>
        /// <returns>The updated strategus hero.</returns>
        /// <response code="200">Updated.</response>
        [HttpPut("self/status")]
        public Task<ActionResult<Result<HeroViewModel>>> UpdateHeroStatus([FromBody] UpdateHeroStatusCommand req)
        {
            req.HeroId = CurrentUser.UserId;
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Buy items from a settlement.
        /// </summary>
        /// <returns>The bought items.</returns>
        /// <response code="200">Bought.</response>
        /// <response code="400">Too far from the settlement, item not available, ...</response>
        [HttpPost("self/items")]
        public Task<ActionResult<Result<ItemStack>>> BuyStrategusItem([FromBody] BuyItemCommand req)
        {
            req.HeroId = CurrentUser.UserId;
            return ResultToActionAsync(Mediator.Send(req));
        }
    }
}
