using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
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
        /// <returns>Current strategus hero, visible heroes and settlements, etc.</returns>
        /// <response code="200">Ok.</response>
        /// <response code="404">User was not registered to strategus.</response>
        [HttpGet("update")]
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
        [HttpPost("heroes")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public Task<ActionResult<Result<StrategusHeroViewModel>>> RegisterStrategusHero([FromBody] CreateStrategusHeroCommand req)
        {
            req.UserId = CurrentUser.UserId;
            return ResultToCreatedAtActionAsync(nameof(GetStrategusUpdate), null, null, Mediator.Send(req));
        }

        /// <summary>
        /// Update strategus hero status.
        /// </summary>
        /// <returns>The updated strategus hero.</returns>
        /// <response code="200">Updated.</response>
        [HttpPut("heroes/self/status")]
        public Task<ActionResult<Result<StrategusHeroViewModel>>> UpdateStrategusHeroStatus([FromBody] UpdateStrategusHeroStatusCommand req)
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
        [HttpPost("heroes/self/items")]
        public Task<ActionResult<Result<StrategusOwnedItemViewModel>>> BuyStrategusItem([FromBody] BuyStrategusItemCommand req)
        {
            req.HeroId = CurrentUser.UserId;
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Get strategus settlements.
        /// </summary>
        [HttpGet("settlements")]
        public Task<ActionResult<Result<IList<StrategusSettlementPublicViewModel>>>> GetSettlements()
            => ResultToActionAsync(Mediator.Send(new GetStrategusSettlementsQuery()));

        /// <summary>
        /// Get strategus settlement shop items.
        /// </summary>
        [HttpGet("settlements/{settlementId}/shop/items")]
        public Task<ActionResult<Result<IList<ItemViewModel>>>> GetSettlementShopItems([FromRoute] int settlementId)
            => ResultToActionAsync(Mediator.Send(new GetStrategusSettlementShopItemsQuery
            {
                HeroId = CurrentUser.UserId,
                SettlementId = settlementId,
            }));

        /// <summary>
        /// Apply as a fighter to a battle.
        /// </summary>
        /// <returns>The application.</returns>
        /// <response code="200">Applied.</response>
        /// <response code="400">Too far from the battle, ...</response>
        [HttpPost("battles/{battleId}/fighters")]
        public Task<ActionResult<Result<StrategusBattleFighterApplicationViewModel>>> ApplyToBattleAsFighter([FromRoute] int battleId)
        {
            ApplyAsFighterToStrategusBattleCommand req = new() { HeroId = CurrentUser.UserId, BattleId = battleId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Apply as a mercenary to a battle.
        /// </summary>
        /// <returns>The application.</returns>
        /// <response code="200">Applied.</response>
        /// <response code="400">Bad request.</response>
        [HttpPost("battles/{battleId}/mercenaries")]
        public Task<ActionResult<Result<StrategusBattleMercenaryApplicationViewModel>>> ApplyToBattleAsMercenary(
            [FromRoute] int battleId, [FromBody] ApplyAsMercenaryToStrategusBattleCommand req)
        {
            req = req with { UserId = CurrentUser.UserId, BattleId = battleId };
            return ResultToActionAsync(Mediator.Send(req));
        }
    }
}
