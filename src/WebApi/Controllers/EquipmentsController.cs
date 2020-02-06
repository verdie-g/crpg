using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trpg.Application.Equipments;
using Trpg.Application.Equipments.Commands;
using Trpg.Application.Equipments.Queries;

namespace Trpg.WebApi.Controllers
{
    public class EquipmentsController : BaseController
    {
        /// <summary>
        /// Gets equipment by its id.
        /// </summary>
        /// <param name="id">Equipment id.</param>
        /// <response code="200">Ok.</response>
        /// <response code="404">Equipment not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentViewModel>> GetEquipment([FromRoute] int id)
        {
            return Ok(await Mediator.Send(new GetEquipmentQuery {EquipmentId = id}));
        }

        /// <summary>
        /// Gets all equipments.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        [ResponseCache(Duration = 60 * 60 * 6)] // 6 hours
        public async Task<ActionResult<IReadOnlyList<EquipmentViewModel>>> GetEquipmentsList()
        {
            return Ok(await Mediator.Send(new GetEquipmentsListQuery()));
        }

        /// <summary>
        /// Creates a new equipment.
        /// </summary>
        /// <param name="cmd">The equipment to create.</param>
        /// <returns>The created equipment.</returns>
        /// <response code="201">Created.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPost, Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<ActionResult<EquipmentViewModel>> CreateEquipment([FromBody] CreateEquipmentCommand cmd)
        {
            var equipment = await Mediator.Send(cmd);
            return CreatedAtAction(nameof(GetEquipment), new {id = equipment.Id}, equipment);
        }

        /// <summary>
        /// Deletes an equipment.
        /// </summary>
        /// <param name="id">Equipment id.</param>
        /// <response code="204">Deleted.</response>
        /// <response code="404">Equipment not found.</response>
        [HttpDelete("{id}"), Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteEquipment([FromRoute] int id)
        {
            await Mediator.Send(new DeleteEquipmentCommand {EquipmentId = id});
            return NoContent();
        }
    }
}