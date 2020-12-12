using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Characters.Commands
{
    public class DeleteCharacterCommand : IMediatorRequest
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<DeleteCharacterCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly ILogger<DeleteCharacterCommand> _logger;

            public Handler(ICrpgDbContext db, ILogger<DeleteCharacterCommand> logger)
            {
                _db = db;
                _logger = logger;
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

                _logger.LogInformation("User '{0}' deleted character '{1}'", req.UserId, req.CharacterId);
                return new Result();
            }
        }
    }
}
