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
    public class BansController : BaseController
    {
        /// <summary>
        /// Gets all bans.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet, Authorize(Roles = "admin,superAdmin")]
        [ResponseCache(Duration = 60 * 60 * 1)] // 1 hour
        public async Task<ActionResult<IList<BanViewModel>>> GetBans()
        {
            var bans = await Mediator.Send(new GetBansListQuery());
            return Ok(bans.Select(b => new BanResponse
            {
                Id = b.Id,
                BannedUser = b.BannedUser,
                Duration = (int)b.Duration.TotalMilliseconds,
                Reason = b.Reason,
                BannedByUser = b.BannedByUser!,
                CreatedAt = b.CreatedAt,
            }));
        }
    }
}
