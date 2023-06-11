using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ItemsController : BaseController
{
    /// <summary>
    /// Gets all enabled items of rank 0.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [ResponseCache(Duration = 60 * 60 * 1)] // 1 hours
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItemsList() =>
        ResultToActionAsync(Mediator.Send(new GetItemsQuery()));

    /// <summary>
    /// Enable/Disable item.
    /// </summary>
    /// <param name="id">Item id.</param>
    /// <param name="req">Enabling value.</param>
    /// <response code="204">Updated.</response>
    /// <response code="400">Bad Request.</response>
    [Authorize(Policy = ModeratorPolicy)]
    [HttpPut("{id}/enable")]
    public Task<ActionResult> EnableItem([FromRoute] string id, [FromBody] EnableItemCommand req)
    {
        req = req with { ItemId = id, UserId = CurrentUser.User!.Id };
        return ResultToActionAsync(Mediator.Send(req));
    }
}
