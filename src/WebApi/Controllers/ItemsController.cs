using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ItemsController : BaseController
{
    /// <summary>
    /// Gets all items of rank 0 (not loomed, not broken).
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItemsList() =>
        ResultToActionAsync(Mediator.Send(new GetItemsQuery()));
}
