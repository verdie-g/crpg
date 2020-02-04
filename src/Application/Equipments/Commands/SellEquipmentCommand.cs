using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Equipments.Commands
{
    public class SellEquipmentCommand : IRequest
    {
        public int EquipmentId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<SellEquipmentCommand>
        {
            private const float SellRatio = 0.66f;

            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(SellEquipmentCommand request, CancellationToken cancellationToken)
            {
                var userEquipment = await _db.UserEquipments
                    .Include(ue => ue.User)
                    .Include(ue => ue.Equipment)
                    .FirstOrDefaultAsync(ue => ue.UserId == request.UserId && ue.EquipmentId == request.EquipmentId, cancellationToken);

                if (userEquipment == null)
                {
                    throw new NotFoundException(nameof(UserEquipment), request.UserId, request.EquipmentId);
                }

                userEquipment.User.Money += (int) (userEquipment.Equipment.Price * SellRatio);
                _db.UserEquipments.Remove(userEquipment);
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}