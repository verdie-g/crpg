using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands;

public record ActivateCharacterCommand : IMediatorRequest
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }
    public bool Active { get; init; }

    internal class Handler : IMediatorRequestHandler<ActivateCharacterCommand>
    {
        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(ActivateCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
            if (character == null)
            {
                return new Result(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (character.ForTournament)
            {
                return new Result(CommonErrors.CharacterForTournament(req.CharacterId));
            }

            character.User!.ActiveCharacterId = req.Active ? character.Id : null;

            await _db.SaveChangesAsync(cancellationToken);
            return new Result();
        }
    }
}
