using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trpg.Application.Characters;
using Trpg.Application.Characters.Commands;
using Trpg.Application.Characters.Queries;
using Trpg.Application.Items;
using Trpg.Application.Items.Commands;
using Trpg.Application.Items.Queries;
using Trpg.Application.Users.Queries;
using Trpg.Web.Models;

namespace Trpg.Web.Controllers
{
    [ApiController]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Gets current user information.
        /// </summary>
        [HttpGet("self")]
        public async Task<IActionResult> GetSelfUser()
        {
            return Ok(await Mediator.Send(new GetUserQuery {UserId = CurrentUser.UserId.Value}));
        }

        /// <summary>
        /// Gets the specified current user's character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="200">Ok.</response>
        /// <response code="404">Character not found.</response>
        [HttpGet("self/characters/{id}")]
        public async Task<ActionResult<CharacterViewModel>> GetUserCharacter([FromRoute] int id)
        {
            return Ok(await Mediator.Send(new GetUserCharacterQuery {CharacterId = id, UserId = CurrentUser.UserId.Value}));
        }

        /// <summary>
        /// Gets all current user's characters.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet("self/characters")]
        public async Task<ActionResult<IReadOnlyList<CharacterViewModel>>> GetUserCharactersList()
        {
            return Ok(await Mediator.Send(new GetUserCharactersListQuery {UserId = CurrentUser.UserId.Value}));
        }

        /// <summary>
        /// Creates a new character for the current user.
        /// </summary>
        /// <param name="req">The character to create.</param>
        /// <returns>The created character.</returns>
        /// <response code="201">Created.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPost("self/characters")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<ActionResult<CharacterViewModel>> CreateCharacter([FromBody] CreateCharacterRequest req)
        {
            var cmd = new CreateCharacterCommand {Name = req.Name, UserId = CurrentUser.UserId.Value};
            var character = await Mediator.Send(cmd);
            return CreatedAtAction(nameof(GetUserCharacter), new {id = character.Id}, character);
        }

        /// <summary>
        /// Updates a character for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">The entire character with the updated values.</param>
        /// <returns>The updated character.</returns>
        /// <response code="200">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}")]
        public async Task<ActionResult<CharacterViewModel>> UpdateCharacter([FromRoute] int id, [FromBody] UpdateCharacterRequest req)
        {
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = id,
                UserId = CurrentUser.UserId.Value,
                Name = req.Name,
            };
            return Ok(await Mediator.Send(cmd));
        }

        /// <summary>
        /// Updates a character's items for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">The entire character's items with the updated values.</param>
        /// <returns>The updated character.</returns>
        /// <response code="200">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}/items")]
        public async Task<ActionResult<CharacterViewModel>> UpdateCharacterItems([FromRoute] int id, [FromBody] UpdateCharacterItemsRequest req)
        {
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = id,
                UserId = CurrentUser.UserId.Value,
                HeadItemId = req.HeadItemId,
                BodyItemId = req.BodyItemId,
                LegsItemId = req.LegsItemId,
                GlovesItemId = req.GlovesItemId,
                Weapon1ItemId = req.Weapon1ItemId,
                Weapon2ItemId = req.Weapon2ItemId,
                Weapon3ItemId = req.Weapon3ItemId,
                Weapon4ItemId = req.Weapon4ItemId,
            };
            return Ok(await Mediator.Send(cmd));
        }

        /// <summary>
        /// Deletes the specified current user's character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="204">Deleted.</response>
        /// <response code="404">Character not found.</response>
        [HttpDelete("self/characters/{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteCharacter([FromRoute] int id)
        {
            await Mediator.Send(new DeleteCharacterCommand {CharacterId = id, UserId = CurrentUser.UserId.Value});
            return NoContent();
        }

        /// <summary>
        /// Gets current user items.
        /// </summary>
        [HttpGet("self/items")]
        public async Task<ActionResult<IReadOnlyList<ItemViewModel>>> GetUserItems()
        {
            var query = new GetUserItemsQuery {UserId = CurrentUser.UserId.Value};
            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Buys item for the current user.
        /// </summary>
        /// <param name="req">The item to buy.</param>
        /// <returns>The bought item.</returns>
        /// <response code="201">Bought.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Item was not found.</response>
        [HttpPost("self/items")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<ActionResult<ItemViewModel>> BuyUserItem([FromBody] BuyItemRequest req)
        {
            var cmd = new BuyItemCommand {ItemId = req.ItemId, UserId = CurrentUser.UserId.Value};
            var item = await Mediator.Send(cmd);
            return CreatedAtAction(nameof(ItemsController.GetItem), "Items", new {id = item.Id}, item);
        }

        /// <summary>
        /// Sells item for the current user.
        /// </summary>
        /// <param name="id">The id of the item to sell.</param>
        /// <response code="204">Sold.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Item was not found.</response>
        [HttpDelete("self/items/{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> SellUserItem([FromRoute] int id)
        {
            await Mediator.Send(new SellItemCommand {ItemId = id, UserId = CurrentUser.UserId.Value});
            return NoContent();
        }
    }
}
