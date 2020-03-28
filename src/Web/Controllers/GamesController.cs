using System.Threading.Tasks;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.Web.Controllers
{
    [Authorize(Roles = "Game")]
    public class GamesController : BaseController
    {
        [HttpPost("ticks")]
        public async Task<ActionResult<TickResponse>> Tick([FromBody] TickCommand tick)
        {
            return Ok(await Mediator.Send(tick));
        }

        [HttpPut("characters")]
        public async Task<ActionResult<GameCharacter>> GetOrCreateCharacter([FromBody] UpsertGameCharacterCommand cmd)
        {
            return Ok(await Mediator.Send(cmd));
        }
    }
}