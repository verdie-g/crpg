using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class DeleteCharacterCommand : IMediatorRequest
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<DeleteCharacterCommand>
        {
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

                return new Result();
            }
        }
    }
}
