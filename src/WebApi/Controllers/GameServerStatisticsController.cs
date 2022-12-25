using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[AllowAnonymous]
[Route("game-server-statistics")]
public class GameServerStatisticsController : BaseController
{
    private readonly IGameServerStatsService _gameServerStatsService;

    public GameServerStatisticsController(IGameServerStatsService gameServerStatsService)
    {
        _gameServerStatsService = gameServerStatsService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<GameServerStats>>> Get()
    {
        var gameServerStats = await _gameServerStatsService.GetGameServerStatsAsync(CancellationToken.None);
        return ResultToAction(new Result<GameServerStats>(gameServerStats));
    }
}
