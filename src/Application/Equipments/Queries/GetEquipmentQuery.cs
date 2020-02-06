using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Equipments.Queries
{
    public class GetEquipmentQuery : IRequest<EquipmentViewModel>
    {
        public int EquipmentId { get; set; }

        public class Handler : IRequestHandler<GetEquipmentQuery, EquipmentViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<EquipmentViewModel> Handle(GetEquipmentQuery request, CancellationToken cancellationToken)
            {
                var equipment = await _db.Equipments
                    .ProjectTo<EquipmentViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);

                if (equipment == null)
                {
                    throw new NotFoundException(nameof(Equipment), request.EquipmentId);
                }

                return equipment;
            }
        }
    }
}
