using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record DeleteCharacterCommand : IMediatorRequest
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<DeleteCharacterCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteCharacterCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IDateTime _dateTime;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IDateTime dateTime, IActivityLogService activityLogService)
        {
            _db = db;
            _dateTime = dateTime;
            _activityLogService = activityLogService;
        }

        public async Task<Result> Handle(DeleteCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.EquippedItems)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new Result(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            character.DeletedAt = _dateTime.UtcNow;
            _db.EquippedItems.RemoveRange(character.EquippedItems);

            _db.ActivityLogs.Add(_activityLogService.CreateCharacterDeletedLog(character.UserId, character.Id,
                character.Generation, character.Level));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' deleted character '{1}'", req.UserId, req.CharacterId);
            return new Result();
        }
    }
}
