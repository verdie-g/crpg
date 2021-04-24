using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Strategus.Commands;
using Crpg.Common;
using Crpg.Sdk.Abstractions.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Strategus
{
    public class Worker : BackgroundService
    {
        private const string TracerOperationName = "strategus.tick";
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<Worker>();
        private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

        private static readonly Func<TimeSpan, IMediatorRequest>[] Behaviors =
        {
            dt => new UpdateStrategusHeroPositionsCommand { DeltaTime = dt },
            dt => new UpdateStrategusHeroTroopsCommand { DeltaTime = dt },
            dt => new UpdateStrategusBattlePhasesCommand { DeltaTime = dt },
        };

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITracer _tracer;

        public Worker(IServiceScopeFactory serviceScopeFactory, ITracer tracer)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _tracer = tracer;
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
            using var span = _tracer.CreateSpan(TracerOperationName);

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
}
