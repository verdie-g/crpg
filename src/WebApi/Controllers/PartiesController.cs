using System.Net;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Commands;
using Crpg.Application.Parties.Models;
using Crpg.Application.Parties.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class PartiesController : BaseController
{
    /// <summary>
    /// Get an update of strategus for the current user.
    /// </summary>
    /// <returns>Current strategus party, visible parties and settlements, etc.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="404">User was not registered to strategus.</response>
    [HttpGet("self/update")]
    public Task<ActionResult<Result<StrategusUpdate>>> GetStrategusUpdate()
    {
        return ResultToActionAsync(Mediator.Send(new GetStrategusUpdateQuery
        {
            PartyId = CurrentUser.UserId,
        }));
    }

    /// <summary>
    /// Register user to strategus.
    /// </summary>
    /// <returns>The new strategus party.</returns>
    /// <response code="201">Registered.</response>
    /// <response code="400">Already registered.</response>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public Task<ActionResult<Result<PartyViewModel>>> RegisterParty([FromBody] CreatePartyCommand req)
    {
        req.UserId = CurrentUser.UserId;
        return ResultToCreatedAtActionAsync(nameof(GetStrategusUpdate), null, null, Mediator.Send(req));
    }

    /// <summary>
    /// Update strategus party status.
    /// </summary>
    /// <returns>The updated strategus party.</returns>
    /// <response code="200">Updated.</response>
    [HttpPut("self/status")]
    public Task<ActionResult<Result<PartyViewModel>>> UpdatePartyStatus([FromBody] UpdatePartyStatusCommand req)
    {
        req.PartyId = CurrentUser.UserId;
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Buy items from a settlement.
    /// </summary>
    /// <returns>The bought items.</returns>
    /// <response code="200">Bought.</response>
    /// <response code="400">Too far from the settlement, item not available, ...</response>
    [HttpPost("self/items")]
    public Task<ActionResult<Result<ItemStack>>> BuySettlementItem([FromBody] BuySettlementItemCommand req)
    {
        req.PartyId = CurrentUser.UserId;
        return ResultToActionAsync(Mediator.Send(req));
    }
}
