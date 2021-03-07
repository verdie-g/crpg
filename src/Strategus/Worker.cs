using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Strategus
{
    public class Worker : BackgroundService
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<Worker>();
        private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

        protected override Task ExecuteAsync(CancellationToken cancellationToken) => GameLoop(cancellationToken);

        private static async Task GameLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopwatch = ValueStopwatch.StartNew();
                await Tick(cancellationToken);
                await SleepUntilNextTick(stopwatch.Elapsed, cancellationToken);
            }
        }

        private static Task Tick(CancellationToken cancellationToken)
        {
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
