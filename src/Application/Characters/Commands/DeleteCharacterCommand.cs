using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Commands
{
    public class DeleteCharacterCommand : IRequest
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<DeleteCharacterCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(DeleteCharacterCommand request, CancellationToken cancellationToken)
            {
                var characterDb = await _db.Characters.FindAsync(request.CharacterId);
                if (characterDb == null)
                {
                    throw new NotFoundException(nameof(Characters), request.CharacterId);
                }

                if (characterDb.UserId != request.UserId)
                {
                    throw new ForbiddenException(nameof(Character), request.CharacterId);
                }

                _db.Characters.Remove(characterDb);
                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
