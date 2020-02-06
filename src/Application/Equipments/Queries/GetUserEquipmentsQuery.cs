using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Application.Equipments.Queries
{
    public class GetUserEquipmentsQuery : IRequest<IReadOnlyList<EquipmentViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserEquipmentsQuery, IReadOnlyList<EquipmentViewModel>>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<EquipmentViewModel>> Handle(GetUserEquipmentsQuery request, CancellationToken cancellationToken)
            {
                return await _db.UserEquipments
                    .Where(ue => ue.UserId == request.UserId)
                    .Include(ue => ue.Equipment)
                    .Select(ue => ue.Equipment)
                    .ProjectTo<EquipmentViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}