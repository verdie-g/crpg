using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Settlements.Commands;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Settlements.Queries;
using Crpg.Application.Strategus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class SettlementsController : BaseController
    {
        /// <summary>
        /// Get strategus settlements.
        /// </summary>
        [HttpGet]
        public Task<ActionResult<Result<IList<SettlementPublicViewModel>>>> GetSettlements()
            => ResultToActionAsync(Mediator.Send(new GetSettlementsQuery()));

        /// <summary>
        /// Give or take garrison items from a settlement.
        /// </summary>
        [HttpPut("{settlementId}/items")]
        public Task<ActionResult<Result<IList<ItemStack>>>> UpdateSettlementItems([FromRoute] int settlementId,
            [FromBody] UpdateSettlementItemsCommand req)
        {
            req = req with { HeroId = CurrentUser.UserId, SettlementId = settlementId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Get strategus settlement shop items.
        /// </summary>
        [HttpGet("{settlementId}/shop/items")]
        public Task<ActionResult<Result<IList<ItemViewModel>>>> GetSettlementShopItems([FromRoute] int settlementId)
            => ResultToActionAsync(Mediator.Send(new GetSettlementShopItemsQuery
            {
                HeroId = CurrentUser.UserId,
                SettlementId = settlementId,
            }));
    }
}
