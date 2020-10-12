using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public class ItemsController : BaseController
    {
        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
        public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItemsList() =>
            ResultToActionAsync(Mediator.Send(new GetItemsListQuery()));
    }
}
