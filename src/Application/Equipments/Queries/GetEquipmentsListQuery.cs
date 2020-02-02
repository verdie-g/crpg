using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Application.Equipments.Queries
{
    public class GetEquipmentsListQuery : IRequest<IReadOnlyList<EquipmentModelView>>
    {
        public class Handler : IRequestHandler<GetEquipmentsListQuery, IReadOnlyList<EquipmentModelView>>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<EquipmentModelView>> Handle(GetEquipmentsListQuery request, CancellationToken cancellationToken)
            {
                return await _db.Equipments
                    .ProjectTo<EquipmentModelView>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
