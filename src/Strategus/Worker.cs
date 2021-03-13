using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Common;
using Crpg.Sdk.Abstractions.Tracing;
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

        private readonly ITracer _tracer;

        public Worker(ITracer tracer)
        {
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

        private Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken)
        {
            using var span = _tracer.CreateSpan(TracerOperationName);
            return Task.CompletedTask;
        }

        private static Task SleepUntilNextTick(TimeSpan elapsed, CancellationToken cancellationToken)
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
