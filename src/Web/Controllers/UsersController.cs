using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trpg.Application.Characters;
using Trpg.Application.Characters.Commands;
using Trpg.Application.Characters.Queries;
using Trpg.Application.Equipments;
using Trpg.Application.Equipments.Commands;
using Trpg.Application.Equipments.Queries;
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
        [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
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
                HeadEquipmentId = req.HeadEquipmentId,
                BodyEquipmentId = req.BodyEquipmentId,
                LegsEquipmentId = req.LegsEquipmentId,
                GlovesEquipmentId = req.GlovesEquipmentId,
                WeaponEquipmentId = req.WeaponEquipmentId,
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
        /// Gets current user equipments.
        /// </summary>
        [HttpGet("self/equipments")]
        public async Task<ActionResult<IReadOnlyList<EquipmentViewModel>>> GetUserEquipments()
        {
            var query = new GetUserEquipmentsQuery {UserId = CurrentUser.UserId.Value};
            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Buys equipment for the current user.
        /// </summary>
        /// <param name="req">The equipment to buy.</param>
        /// <returns>The bought equipment.</returns>
        /// <response code="201">Bought.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Equipment was not found.</response>
        [HttpPost("self/equipments")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<ActionResult<EquipmentViewModel>> BuyUserEquipment([FromBody] BuyEquipmentRequest req)
        {
            var cmd = new BuyEquipmentCommand {EquipmentId = req.EquipmentId, UserId = CurrentUser.UserId.Value};
            var equipment = await Mediator.Send(cmd);
            return CreatedAtAction(nameof(EquipmentsController.GetEquipment), "Equipments", new {id = equipment.Id}, equipment);
        }

        /// <summary>
        /// Sells equipment for the current user.
        /// </summary>
        /// <param name="id">The id of the equipment to sell.</param>
        /// <response code="204">Sold.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Equipment was not found.</response>
        [HttpDelete("self/equipments/{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> SellUserEquipment([FromRoute] int id)
        {
            await Mediator.Send(new SellEquipmentCommand {EquipmentId = id, UserId = CurrentUser.UserId.Value});
            return NoContent();
        }
    }
}
