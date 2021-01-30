using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Commands;
using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class ClansController : BaseController
    {
        /// <summary>
        /// Gets a clan from its id.
        /// </summary>
        /// <response code="200">Ok.</response>
        /// <response code="404">Clan was not found.</response>
        [HttpGet("{id}")]
        public Task<ActionResult<Result<ClanViewModel>>> GetClan(int id) =>
            ResultToActionAsync(Mediator.Send(new GetClanQuery { ClanId = id }));

        /// <summary>
        /// Gets all clans.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet]
        public Task<ActionResult<Result<IList<ClanViewModel>>>> GetClans() =>
            ResultToActionAsync(Mediator.Send(new GetClansQuery()));

        /// <summary>
        /// Creates a clan.
        /// </summary>
        /// <param name="clan">Clan info.</param>
        /// <returns>The created clan.</returns>
        /// <response code="201">Created.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPost]
        public Task<ActionResult<Result<ClanViewModel>>> CreateClan([FromBody] CreateClanCommand clan)
        {
            clan.UserId = CurrentUser.UserId;
            return ResultToCreatedAtActionAsync(nameof(GetClan), null, b => new { id = b.Id },
                Mediator.Send(clan));
        }

        /// <summary>
        /// Kick a clan member of leave a clan.
        /// </summary>
        /// <returns>The created clan.</returns>
        /// <response code="204">Kicked or left.</response>
        /// <response code="400">Bad Request.</response>
        [HttpDelete("{clanId}/members/{userId}")]
        public Task<ActionResult> DeleteClan(int clanId, int userId)
        {
            return ResultToActionAsync(Mediator.Send(new KickClanMemberCommand
            {
                UserId = CurrentUser.UserId,
                ClanId = clanId,
                KickedUserId = userId,
            }, CancellationToken.None));
        }

        /// <summary>
        /// Invite user to clan or request to join a clan.
        /// </summary>
        /// <returns>The created or existing invitation.</returns>
        /// <response code="201">Invitation created.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPost("{clanId}/invitations")]
        public Task<ActionResult<Result<ClanInvitationViewModel>>> InviteToClan([FromQuery] int clanId, [FromBody] InviteClanMemberCommand invite)
        {
            invite.UserId = CurrentUser.UserId;
            invite.ClanId = clanId;
            return ResultToCreatedAtActionAsync("", null, i => new { id = i.Id },
                Mediator.Send(invite));
        }
    }
}
