using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
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

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteCharacterCommand req, CancellationToken cancellationToken)
        {
            var characterDb =
                await _db.Characters.FirstOrDefaultAsync(c =>
                    c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (characterDb == null)
            {
                return new Result(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            _db.Characters.Remove(characterDb);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' deleted character '{1}'", req.UserId, req.CharacterId);
            return new Result();
        }
    }
}
