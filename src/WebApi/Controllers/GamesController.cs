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
        [HttpPost("ticks")]
        public async Task<ActionResult<RewardResponse>> Reward([FromBody] RewardCommand reward)
        {
            return Ok(await Mediator.Send(reward));
        }

        [HttpPut("users")]
        public async Task<ActionResult<GameUser>> GetOrCreateUser([FromBody] UpsertGameUserCommand cmd)
        {
            return Ok(await Mediator.Send(cmd));
        }
    }
}