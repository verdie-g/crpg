using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trpg.Application.Items.Commands;
using Trpg.Application.Items.Models;
using Trpg.Application.Items.Queries;

namespace Trpg.Web.Controllers
{
    public class ItemsController : BaseController
    {
        /// <summary>
        /// Gets item by its id.
        /// </summary>
        /// <param name="id">Item id.</param>
        /// <response code="200">Ok.</response>
        /// <response code="404">Item not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemViewModel>> GetItem([FromRoute] int id)
        {
            return Ok(await Mediator.Send(new GetItemQuery {ItemId = id}));
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
        public async Task<ActionResult<IReadOnlyList<ItemViewModel>>> GetItemsList()
        {
            return Ok(await Mediator.Send(new GetItemsListQuery()));
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="cmd">The item to create.</param>
        /// <returns>The created item.</returns>
        /// <response code="201">Created.</response>
        [HttpPost, Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<ActionResult<ItemViewModel>> CreateItem([FromBody] CreateItemsCommand cmd)
        {
            await Mediator.Send(cmd);
            return StatusCode((int) HttpStatusCode.Created);
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="id">Item id.</param>
        /// <response code="204">Deleted.</response>
        /// <response code="404">Item not found.</response>
        [HttpDelete("{id}"), Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteItem([FromRoute] int id)
        {
            await Mediator.Send(new DeleteItemCommand {ItemId = id});
            return NoContent();
        }
    }
}