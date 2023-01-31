using Crpg.Application.ActivityLogs.Commands;
using MediatR;

namespace Crpg.WebApi.Workers;

internal class ActivityLogsCleanerWorker : BackgroundService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<ActivityLogsCleanerWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ActivityLogsCleanerWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new DeleteOldActivityLogsCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while cleaning activity logs");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
