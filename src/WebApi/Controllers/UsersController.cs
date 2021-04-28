using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Crpg.Application.Bans.Models;
using Crpg.Application.Bans.Queries;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Crpg.Application.Users.Commands;
using Crpg.Application.Users.Models;
using Crpg.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [Authorize(Policy = UserPolicy)]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Gets current user information.
        /// </summary>
        [HttpGet("self")]
        public Task<ActionResult<Result<UserViewModel>>> GetUser()
            => ResultToActionAsync(Mediator.Send(new GetUserQuery { UserId = CurrentUser.UserId }));

        /// <summary>
        /// Deletes current user.
        /// </summary>
        /// <response code="204">Deleted.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete("self")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<ActionResult> DeleteUser() =>
            ResultToActionAsync(Mediator.Send(new DeleteUserCommand { UserId = CurrentUser.UserId }));

        /// <summary>
        /// Gets the specified current user's character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="200">Ok.</response>
        /// <response code="404">Character not found.</response>
        [HttpGet("self/characters/{id}")]
        public Task<ActionResult<Result<CharacterViewModel>>> GetUserCharacter([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new GetUserCharacterQuery
                { CharacterId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Gets all current user's characters.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet("self/characters")]
        public Task<ActionResult<Result<IList<CharacterViewModel>>>> GetUserCharactersList() =>
            ResultToActionAsync(Mediator.Send(new GetUserCharactersQuery { UserId = CurrentUser.UserId }));

        /// <summary>
        /// Updates a character for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">The entire character with the updated values.</param>
        /// <returns>The updated character.</returns>
        /// <response code="200">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}")]
        public Task<ActionResult<Result<CharacterViewModel>>> UpdateCharacter([FromRoute] int id,
            [FromBody] UpdateCharacterCommand req)
        {
            req = req with { UserId = CurrentUser.UserId, CharacterId = id };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Get character statistics for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <returns>The character statistics.</returns>
        /// <response code="200">Ok.</response>
        [HttpGet("self/characters/{id}/statistics")]
        public Task<ActionResult<Result<CharacterStatisticsViewModel>>> GetCharacterStatistics([FromRoute] int id)
        {
            return ResultToActionAsync(Mediator.Send(new GetUserCharacterStatisticsQuery
            {
                UserId = CurrentUser.UserId,
                CharacterId = id,
            }));
        }

        /// <summary>
        /// Updates character statistics for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="stats">The character statistics with the updated values.</param>
        /// <returns>The updated character statistics.</returns>
        /// <response code="200">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}/statistics")]
        public Task<ActionResult<Result<CharacterStatisticsViewModel>>> UpdateCharacterStatistics([FromRoute] int id,
            [FromBody] CharacterStatisticsViewModel stats)
        {
            var cmd = new UpdateCharacterStatisticsCommand
            {
                UserId = CurrentUser.UserId,
                CharacterId = id,
                Statistics = stats,
            };
            return ResultToActionAsync(Mediator.Send(cmd));
        }

        /// <summary>
        /// Convert character statistics for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">The conversion to perform.</param>
        /// <returns>The updated character statistics.</returns>
        /// <response code="200">Conversion performed.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}/statistics/convert")]
        public Task<ActionResult<Result<CharacterStatisticsViewModel>>> ConvertCharacterStatistics([FromRoute] int id,
            [FromBody] ConvertCharacterStatisticsCommand req)
        {
            req = req with { CharacterId = id, UserId = CurrentUser.UserId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Get character items for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <returns>The character items.</returns>
        /// <response code="200">Ok.</response>
        [HttpGet("self/characters/{id}/items")]
        public Task<ActionResult<Result<IList<EquippedItemViewModel>>>> GetCharacterItems([FromRoute] int id)
        {
            return ResultToActionAsync(Mediator.Send(new GetUserCharacterItemsQuery
            {
                UserId = CurrentUser.UserId,
                CharacterId = id,
            }));
        }

        /// <summary>
        /// Updates a character's items for the current user.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">Item slots that changed.</param>
        /// <returns>The updated character items.</returns>
        /// <response code="200">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}/items")]
        public Task<ActionResult<Result<IList<EquippedItemViewModel>>>> UpdateCharacterItems([FromRoute] int id,
            [FromBody] UpdateCharacterItemsCommand req)
        {
            req = req with { CharacterId = id, UserId = CurrentUser.UserId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Switch on/off auto-repairing for a character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="req">Auto repair value.</param>
        /// <response code="204">Updated.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/characters/{id}/auto-repair")]
        public Task<ActionResult> SwitchCharacterAutoRepair([FromRoute] int id,
            [FromBody] SwitchCharacterAutoRepairCommand req)
        {
            req = req with { CharacterId = id, UserId = CurrentUser.UserId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Retires character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="200">Retired.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Character not found.</response>
        [HttpPut("self/characters/{id}/retire")]
        public Task<ActionResult<Result<CharacterViewModel>>> RetireCharacter([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new RetireCharacterCommand { CharacterId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Respecializes character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="200">Respecialized.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Character not found.</response>
        [HttpPut("self/characters/{id}/respecialize")]
        public Task<ActionResult<Result<CharacterViewModel>>> RespecializeCharacter([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new RespecializeCharacterCommand { CharacterId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Deletes the specified current user's character.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <response code="204">Deleted.</response>
        /// <response code="404">Character not found.</response>
        [HttpDelete("self/characters/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<ActionResult> DeleteCharacter([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new DeleteCharacterCommand { CharacterId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Gets owned items.
        /// </summary>
        [HttpGet("self/items")]
        public Task<ActionResult<Result<IList<ItemViewModel>>>> GetOwnedItems()
        {
            var query = new GetOwnedItemsQuery { UserId = CurrentUser.UserId };
            return ResultToActionAsync(Mediator.Send(query));
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
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public Task<ActionResult<Result<ItemViewModel>>> BuyItem([FromBody] BuyItemCommand req)
        {
            req = req with { UserId = CurrentUser.UserId };
            return ResultToCreatedAtActionAsync(nameof(ItemsController.GetItemsList), "Items", i => new { id = i.Id },
                Mediator.Send(req));
        }

        /// <summary>
        /// Repair or loom item.
        /// </summary>
        /// <param name="id">Item id.</param>
        /// <returns>The upgraded item.</returns>
        /// <response code="200">Upgraded.</response>
        /// <response code="400">Bad Request.</response>
        [HttpPut("self/items/{id}/upgrade")]
        public Task<ActionResult<Result<ItemViewModel>>> UpgradeItem([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new UpgradeItemCommand { ItemId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Sells item for the current user.
        /// </summary>
        /// <param name="id">The id of the item to sell.</param>
        /// <response code="204">Sold.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Item was not found.</response>
        [HttpDelete("self/items/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<ActionResult> SellOwnedItem([FromRoute] int id) =>
            ResultToActionAsync(Mediator.Send(new SellItemCommand { ItemId = id, UserId = CurrentUser.UserId }));

        /// <summary>
        /// Gets user clan or null.
        /// </summary>
        [HttpGet("self/clans")]
        public Task<ActionResult<Result<ClanViewModel>>> GetUserClan()
        {
            GetUserClanQuery req = new() { UserId = CurrentUser.UserId };
            return ResultToActionAsync(Mediator.Send(req));
        }

        /// <summary>
        /// Gets all current user's bans.
        /// </summary>
        /// <response code="200">Ok.</response>
        [HttpGet("self/bans")]
        public Task<ActionResult<Result<IList<BanViewModel>>>> GetUserBans() =>
            ResultToActionAsync(Mediator.Send(new GetUserBansQuery { UserId = CurrentUser.UserId }));
    }
}
