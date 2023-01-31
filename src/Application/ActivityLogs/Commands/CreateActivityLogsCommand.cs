using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.ActivityLogs;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.ActivityLogs.Commands;

public record CreateActivityLogsCommand : IMediatorRequest
{
    public IList<ActivityLogViewModel> ActivityLogs { get; init; } = Array.Empty<ActivityLogViewModel>();

    internal class Handler : IMediatorRequestHandler<CreateActivityLogsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateActivityLogsCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(CreateActivityLogsCommand req, CancellationToken cancellationToken)
        {
            var activityLogs = req.ActivityLogs.Select(l => new ActivityLog
            {
                Type = l.Type,
                UserId = l.UserId,
                Metadata = l.Metadata.Select(m => new ActivityLogMetadata(m.Key, m.Value)).ToList(),
            });

            _db.ActivityLogs.AddRange(activityLogs);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Inserted {0} activity logs", req.ActivityLogs.Count);

            return Result.NoErrors;
        }
    }
}
