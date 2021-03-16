using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public class GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
    {
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusUpdate>> Handle(GetStrategusUpdateQuery req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.StrategusUser)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

                if (user == null)
                {
                    return new Result<StrategusUpdate>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.StrategusUser == null)
                {
                    return new Result<StrategusUpdate>(CommonErrors.UserNotRegisteredToStrategus(req.UserId));
                }

                var visibleUsers = await _db.StrategusUsers
                    .Where(u => u.UserId != user.Id && u.Position.Distance(user.StrategusUser.Position) < _strategusMap.ViewDistance)
                    .ProjectTo<StrategusUserPublicViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                var visibleSettlements = await _db.StrategusSettlements
                    .Where(u => u.Position.Distance(user.StrategusUser.Position) < _strategusMap.ViewDistance)
                    .ProjectTo<StrategusSettlementViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new Result<StrategusUpdate>(new StrategusUpdate
                {
                    User = _mapper.Map<StrategusUserViewModel>(user.StrategusUser),
                    VisibleUsers = visibleUsers,
                    VisibleSettlements = visibleSettlements,
                });
            }
        }
    }
}
