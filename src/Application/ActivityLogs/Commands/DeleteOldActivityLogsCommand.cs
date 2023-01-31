using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.ActivityLogs.Commands;

public record DeleteOldActivityLogsCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<DeleteOldActivityLogsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteOldActivityLogsCommand>();
        private static readonly TimeSpan LogRetention = TimeSpan.FromDays(30);

        private readonly ICrpgDbContext _db;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IDateTime dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async Task<Result> Handle(DeleteOldActivityLogsCommand req, CancellationToken cancellationToken)
        {
            var limit = _dateTime.UtcNow - LogRetention;
            var activityLogs = await _db.ActivityLogs
                .Where(l => l.CreatedAt < limit)
                .Include(l => l.Metadata)
                .ToArrayAsync(cancellationToken);

            // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our
            // tests (https://github.com/dotnet/efcore/issues/30185).
            _db.ActivityLogs.RemoveRange(activityLogs);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} old activity logs were cleaned out", activityLogs.Length);

            return Result.NoErrors;
        }
    }
}
