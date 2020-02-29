using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                var characterDb =
                    await _db.Characters.FirstOrDefaultAsync(c =>
                        c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (characterDb == null)
                {
                    throw new NotFoundException(nameof(Characters), request.CharacterId);
                }

                _db.Characters.Remove(characterDb);
                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
