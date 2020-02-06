using System.Linq;
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
    public class BuyEquipmentCommand : IRequest<EquipmentViewModel>
    {
        public int EquipmentId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<BuyEquipmentCommand, EquipmentViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<EquipmentViewModel> Handle(BuyEquipmentCommand request, CancellationToken cancellationToken)
            {
                var equipment = await _db.Equipments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);
                if (equipment == null)
                {
                    throw new NotFoundException(nameof(Equipment), request.EquipmentId);
                }

                var user = await _db.Users
                    .Include(u => u.UserEquipments)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                if (user.UserEquipments.Any(e => e.EquipmentId == request.EquipmentId))
                {
                    throw new BadRequestException("User already owns this equipment");
                }

                if (user.Money < equipment.Price)
                {
                    throw new BadRequestException("User doesn't have enough money");
                }

                user.Money -= equipment.Price;
                user.UserEquipments.Add(new UserEquipment {UserId = request.UserId, EquipmentId = request.EquipmentId});
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<EquipmentViewModel>(equipment);
            }
        }
    }
}