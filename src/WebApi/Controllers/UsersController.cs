using System.Net;
using Crpg.Application.Restrictions.Models;
using Crpg.Application.Restrictions.Queries;
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

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class UsersController : BaseController
{
    /// <summary>
    /// Gets current user information.
    /// </summary>
    [HttpGet("self")]
    public Task<ActionResult<Result<UserViewModel>>> GetUser()
        => ResultToActionAsync(Mediator.Send(new GetUserQuery { UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Deletes current user.
    /// </summary>
    /// <response code="204">Deleted.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("self")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<ActionResult> DeleteUser() =>
        ResultToActionAsync(Mediator.Send(new DeleteUserCommand { UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Gets the specified current user's character.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <response code="200">Ok.</response>
    /// <response code="404">Character not found.</response>
    [HttpGet("self/characters/{id}")]
    public Task<ActionResult<Result<CharacterViewModel>>> GetUserCharacter([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new GetUserCharacterQuery
            { CharacterId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Gets all current user's characters.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet("self/characters")]
    public Task<ActionResult<Result<IList<CharacterViewModel>>>> GetUserCharactersList() =>
        ResultToActionAsync(Mediator.Send(new GetUserCharactersQuery { UserId = CurrentUser.User!.Id }));

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
        req = req with { UserId = CurrentUser.User!.Id, CharacterId = id };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get character characteristics for the current user.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <returns>The character characteristics.</returns>
    /// <response code="200">Ok.</response>
    [HttpGet("self/characters/{id}/characteristics")]
    public Task<ActionResult<Result<CharacterCharacteristicsViewModel>>> GetCharacterCharacteristics([FromRoute] int id)
    {
        return ResultToActionAsync(Mediator.Send(new GetUserCharacterCharacteristicsQuery
        {
            UserId = CurrentUser.User!.Id,
            CharacterId = id,
        }));
    }

    /// <summary>
    /// Updates character characteristics for the current user.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <param name="stats">The character characteristics with the updated values.</param>
    /// <returns>The updated character characteristics.</returns>
    /// <response code="200">Updated.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPut("self/characters/{id}/characteristics")]
    public Task<ActionResult<Result<CharacterCharacteristicsViewModel>>> UpdateCharacterCharacteristics([FromRoute] int id,
        [FromBody] CharacterCharacteristicsViewModel stats)
    {
        UpdateCharacterCharacteristicsCommand cmd = new()
        {
            UserId = CurrentUser.User!.Id,
            CharacterId = id,
            Characteristics = stats,
        };
        return ResultToActionAsync(Mediator.Send(cmd));
    }

    /// <summary>
    /// Convert character characteristics for the current user.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <param name="req">The conversion to perform.</param>
    /// <returns>The updated character characteristics.</returns>
    /// <response code="200">Conversion performed.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPut("self/characters/{id}/characteristics/convert")]
    public Task<ActionResult<Result<CharacterCharacteristicsViewModel>>> ConvertCharacterCharacteristics([FromRoute] int id,
        [FromBody] ConvertCharacterCharacteristicsCommand req)
    {
        req = req with { CharacterId = id, UserId = CurrentUser.User!.Id };
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
            UserId = CurrentUser.User!.Id,
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
        req = req with { CharacterId = id, UserId = CurrentUser.User!.Id };
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
        req = req with { CharacterId = id, UserId = CurrentUser.User!.Id };
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
            UserId = CurrentUser.User!.Id,
            CharacterId = id,
        }));
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
        ResultToActionAsync(Mediator.Send(new RetireCharacterCommand { CharacterId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Respecializes character.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <response code="200">Respecialized.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="404">Character not found.</response>
    [HttpPut("self/characters/{id}/respecialize")]
    public Task<ActionResult<Result<CharacterViewModel>>> RespecializeCharacter([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new RespecializeCharacterCommand { CharacterId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Deletes the specified current user's character.
    /// </summary>
    /// <param name="id">Character id.</param>
    /// <response code="204">Deleted.</response>
    /// <response code="404">Character not found.</response>
    [HttpDelete("self/characters/{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<ActionResult> DeleteCharacter([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new DeleteCharacterCommand { CharacterId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Gets owned items.
    /// </summary>
    [HttpGet("self/items")]
    public Task<ActionResult<Result<IList<UserItemViewModel>>>> GetUserItems()
    {
        GetUserItemsQuery query = new() { UserId = CurrentUser.User!.Id };
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
    public Task<ActionResult<Result<UserItemViewModel>>> BuyItem([FromBody] BuyItemCommand req)
    {
        req = req with { UserId = CurrentUser.User!.Id };
        return ResultToCreatedAtActionAsync(nameof(GetUserItems), null, ui => new { id = ui.Id },
            Mediator.Send(req));
    }

    /// <summary>
    /// Repair or loom item.
    /// </summary>
    /// <param name="id">User item id.</param>
    /// <returns>The upgraded item.</returns>
    /// <response code="200">Upgraded.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPut("self/items/{id}/upgrade")]
    public Task<ActionResult<Result<UserItemViewModel>>> UpgradeUserItem([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new UpgradeUserItemCommand { UserItemId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Sells item for the current user.
    /// </summary>
    /// <param name="id">The id of the user item to sell.</param>
    /// <response code="204">Sold.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="404">Item was not found.</response>
    [HttpDelete("self/items/{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<ActionResult> SellUserItem([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new SellUserItemCommand { UserItemId = id, UserId = CurrentUser.User!.Id }));

    /// <summary>
    /// Gets user clan or null.
    /// </summary>
    [HttpGet("self/clans")]
    public Task<ActionResult<Result<ClanViewModel>>> GetUserClan()
    {
        GetUserClanQuery req = new() { UserId = CurrentUser.User!.Id };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Gets all current user's restrictions.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet("self/restrictions")]
    public Task<ActionResult<Result<IList<RestrictionViewModel>>>> GetUserRestrictions() =>
        ResultToActionAsync(Mediator.Send(new GetUserRestrictionsQuery { UserId = CurrentUser.User!.Id }));
}
