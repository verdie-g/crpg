using System.Diagnostics;
using System.Reflection;
using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Parties.Commands;
using Crpg.Common;
using MediatR;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.WebApi.Workers;

/// <summary>Worker to update the strategus map (movements, battles, ...).</summary>
public class LeaderboardWorker : BackgroundService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<LeaderboardWorker>();
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(10);
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LeaderboardWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) => LeaderBoardLoop(cancellationToken);

    private async Task LeaderBoardLoop(CancellationToken cancellationToken)
    {
        TimeSpan deltaTime = TickInterval;
        while (!cancellationToken.IsCancellationRequested)
        {
            var stopwatch = ValueStopwatch.StartNew();
            // There is no guarantee that Task.Delay delays the exact given time in SleepUntilNextTick so a delta
            // time is computed to get the real delay and it is passed to Tick instead of passing TickInterval.
            await Tick(deltaTime, cancellationToken);
            await SleepUntilNextTick(stopwatch.Elapsed, cancellationToken);
            deltaTime = stopwatch.Elapsed;
        }
    }

    private async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken)
    {
        // ICrpgDbContext, used by most mediator handlers has a scoped lifetime which means that it is created once
        // for each ASP.NET Core request. But in a worker there is no notion of request so we manually create the
        // scope here.
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new UpdateLeaderboardCommand(), cancellationToken);
    }

    private Task SleepUntilNextTick(TimeSpan elapsed, CancellationToken cancellationToken)
    {
        TimeSpan remainingTimeInTick = TickInterval - elapsed;
        if (remainingTimeInTick > TimeSpan.Zero)
        {
            return Task.Delay(remainingTimeInTick, cancellationToken);
        }

        Logger.LogWarning("Leaderboard  took more time than expected: {0} (expected less than {1})",
            elapsed, TickInterval);
        return Task.CompletedTask;
    }
}
