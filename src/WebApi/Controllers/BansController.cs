using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Crpg.Application.Bans.Commands;
using Crpg.Application.Bans.Models;
using Crpg.Application.Bans.Queries;
using Crpg.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Roles = "admin,superAdmin")]
    public class BansController : BaseController
    {
        /// <summary>
        /// Gets all bans.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        [ResponseCache(Duration = 60 * 60 * 1)] // 1 hour
        public async Task<ActionResult<IList<BanViewModel>>> GetBans()
        {
            return Ok(await Mediator.Send(new GetBansListQuery()));
        }

        /// <summary>
        /// Bans an user. If a ban already exists for the user, it is overriden. Use a duration of 0 to unban.
        /// </summary>
        /// <param name="req">Ban info.</param>
        /// <returns>The ban object.</returns>
        /// <response code="201">Banned.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">User was not found.</response>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<BanViewModel>> BanUser([FromBody] BanCommand req)
        {
            req.BannedByUserId = CurrentUser.UserId;
            var ban = await Mediator.Send(req);
            return StatusCode(StatusCodes.Status201Created, ban);
        }
    }
}
