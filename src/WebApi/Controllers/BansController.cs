using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crpg.Application.Bans.Models;
using Crpg.Application.Bans.Queries;
using Crpg.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
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
    }
}
