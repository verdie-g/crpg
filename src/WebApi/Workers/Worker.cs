using System.Diagnostics;
using System.Reflection;
using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Heroes.Commands;
using Crpg.Common;
using MediatR;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.WebApi.Workers;

/// <summary>Worker to update the strategus map (movements, battles, ...).</summary>
public class StrategusWorker : BackgroundService
{
    private static readonly AssemblyName AssemblyName = typeof(StrategusWorker).Assembly.GetName();
    private static readonly string InstrumentationName = AssemblyName.Name!;
    private static readonly string InstrumentationVersion = AssemblyName.Version!.ToString();
    private static readonly ActivitySource ActivitySource = new(InstrumentationName, InstrumentationVersion);
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<StrategusWorker>();
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    private static readonly Func<TimeSpan, IMediatorRequest>[] Behaviors =
    {
        dt => new UpdateHeroPositionsCommand { DeltaTime = dt },
        dt => new UpdateHeroTroopsCommand { DeltaTime = dt },
        dt => new UpdateBattlePhasesCommand { DeltaTime = dt },
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StrategusWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) => GameLoop(cancellationToken);

    private async Task GameLoop(CancellationToken cancellationToken)
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
        using var span = ActivitySource.StartActivity("crpg.strategus.tick");

        // ICrpgDbContext, used by most mediator handlers has a scoped lifetime which means that it is created once
        // for each ASP.NET Core request. But in a worker there is no notion of request so we manually create the
        // scope here.
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        foreach (var behavior in Behaviors)
        {
            await mediator.Send(behavior(deltaTime), cancellationToken);
        }
    }

    private Task SleepUntilNextTick(TimeSpan elapsed, CancellationToken cancellationToken)
    {
        TimeSpan remainingTimeInTick = TickInterval - elapsed;
        if (remainingTimeInTick > TimeSpan.Zero)
        {
            return Task.Delay(remainingTimeInTick, cancellationToken);
        }

        Logger.LogWarning("Strategus tick took more time than expected: {0} (expected less than {1})",
            elapsed, TickInterval);
        return Task.CompletedTask;
    }
}
