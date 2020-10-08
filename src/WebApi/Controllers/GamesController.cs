using System.Threading.Tasks;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Roles = "Game")]
    public class GamesController : BaseController
    {
        /// <summary>
        /// All-in-One endpoint to get or create users with character, give gold and experience, and break/repair items.
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<UpdateGameResult>> Update([FromBody] UpdateGameCommand cmd)
        {
            return Ok(await Mediator.Send(cmd));
        }
    }
}
