using Crpg.Application.Battles.Commands;
using Crpg.Application.Battles.Models;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class BattlesController : BaseController
{
    /// <summary>
    /// Get strategus battles.
    /// </summary>
    [HttpGet]
    public Task<ActionResult<Result<IList<BattleDetailedViewModel>>>> GetBattles([FromQuery] Region region,
        [FromQuery(Name = "phase[]")] BattlePhase[] phases)
        => ResultToActionAsync(Mediator.Send(new GetBattlesQuery
        {
            Region = region,
            Phases = phases,
        }));

    /// <summary>
    /// Get strategus battle.
    /// </summary>
    [HttpGet("{battleId}")]
    public Task<ActionResult<Result<BattleViewModel>>> GetBattles([FromRoute] int battleId) =>
        ResultToActionAsync(Mediator.Send(new GetBattleQuery
        {
            BattleId = battleId,
        }));

    /// <summary>
    /// Get battle fighters.
    /// </summary>
    /// <returns>The fighters.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("{battleId}/fighters")]
    public Task<ActionResult<Result<IList<BattleFighterViewModel>>>> GetBattleFighters([FromRoute] int battleId)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleFightersQuery
        {
            BattleId = battleId,
        }));
    }

    /// <summary>
    /// Apply as a fighter to a battle.
    /// </summary>
    /// <returns>The application.</returns>
    /// <response code="200">Applied.</response>
    /// <response code="400">Too far from the battle, ...</response>
    [HttpPost("{battleId}/fighters")]
    public Task<ActionResult<Result<BattleFighterApplicationViewModel>>> ApplyToBattleAsFighter([FromRoute] int battleId)
    {
        ApplyAsFighterToBattleCommand req = new() { PartyId = CurrentUser.UserId, BattleId = battleId };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get battle fighter applications.
    /// </summary>
    /// <returns>
    /// If the party is a command of the battle it will return all applications of their battle side, else it returns
    /// only the applications of the party.
    /// </returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("{battleId}/fighter-applications")]
    public Task<ActionResult<Result<IList<BattleFighterApplicationViewModel>>>> GetBattleFighterApplications(
        [FromRoute] int battleId, [FromQuery(Name = "status[]")] BattleFighterApplicationStatus[] statuses)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleFighterApplicationsQuery
        {
            PartyId = CurrentUser.UserId,
            BattleId = battleId,
            Statuses = statuses,
        }));
    }

    /// <summary>
    /// Accept/Decline battle fighter application.
    /// </summary>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPut("{battleId}/fighter-applications/{applicationId}/response")]
    public Task<ActionResult<Result<BattleFighterApplicationViewModel>>> RespondToBattleFighterApplication(
        [FromRoute] int battleId, [FromRoute] int applicationId, [FromBody] RespondToBattleFighterApplicationCommand req)
    {
        req = req with { PartyId = CurrentUser.UserId, FighterApplicationId = applicationId };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get battle mercenaries.
    /// </summary>
    /// <returns>The mercenaries.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("{battleId}/mercenaries")]
    public Task<ActionResult<Result<IList<BattleMercenaryViewModel>>>> GetBattleMercenaries([FromRoute] int battleId)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleMercenariesQuery
        {
            UserId = CurrentUser.UserId,
            BattleId = battleId,
        }));
    }

    /// <summary>
    /// Get battle mercenary applications.
    /// </summary>
    /// <returns>
    /// If the user is a fighter of the battle it will return all applications of their battle side, else it returns
    /// only the applications of the user.
    /// </returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("{battleId}/mercenary-applications")]
    public Task<ActionResult<Result<IList<BattleMercenaryApplicationViewModel>>>> GetBattleMercenaryApplications(
        [FromRoute] int battleId, [FromQuery(Name = "status[]")] BattleMercenaryApplicationStatus[] statuses)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleMercenaryApplicationsQuery
        {
            UserId = CurrentUser.UserId,
            BattleId = battleId,
            Statuses = statuses,
        }));
    }

    /// <summary>
    /// Apply as a mercenary to a battle.
    /// </summary>
    /// <returns>The application.</returns>
    /// <response code="200">Applied.</response>
    /// <response code="400">Bad request.</response>
    [HttpPost("{battleId}/mercenary-applications")]
    public Task<ActionResult<Result<BattleMercenaryApplicationViewModel>>> ApplyToBattleAsMercenary(
        [FromRoute] int battleId, [FromBody] ApplyAsMercenaryToBattleCommand req)
    {
        req = req with { UserId = CurrentUser.UserId, BattleId = battleId };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Accept/Decline battle mercenary application.
    /// </summary>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPut("{battleId}/mercenary-applications/{applicationId}/response")]
    public Task<ActionResult<Result<BattleMercenaryApplicationViewModel>>> RespondToBattleMercenaryApplication(
        [FromRoute] int battleId, [FromRoute] int applicationId, [FromBody] RespondToBattleMercenaryApplicationCommand req)
    {
        req = req with { PartyId = CurrentUser.UserId, MercenaryApplicationId = applicationId };
        return ResultToActionAsync(Mediator.Send(req));
    }
}
