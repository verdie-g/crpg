using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Roles = "user,admin,superAdmin")]
    public class ItemsController : BaseController
    {
        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
        public async Task<ActionResult<IList<ItemViewModel>>> GetItemsList()
        {
            return Ok(await Mediator.Send(new GetItemsListQuery()));
        }
    }
}