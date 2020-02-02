using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Application.Equipments.Commands
{
    public class DeleteEquipmentCommand : IRequest
    {
        public int EquipmentId { get; set; }

        public class Handler : IRequestHandler<DeleteEquipmentCommand>
        {
            private readonly ITrpgDbContext _db;

            public Handler(ITrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(DeleteEquipmentCommand request, CancellationToken cancellationToken)
            {
                var equipmentDb = await _db.Equipments.FindAsync(request.EquipmentId);
                if (equipmentDb == null)
                {
                    throw new NotFoundException(nameof(Equipments), request.EquipmentId);
                }

                _db.Equipments.Remove(equipmentDb);
                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
