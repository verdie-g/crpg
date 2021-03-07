using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public class GetStrategusUserQuery : IMediatorRequest<StrategusUserViewModel>
    {
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<GetStrategusUserQuery, StrategusUserViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<StrategusUserViewModel>> Handle(GetStrategusUserQuery req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.StrategusUser)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

                if (user == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.StrategusUser == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotRegisteredToStrategus(req.UserId));
                }

                return new Result<StrategusUserViewModel>(_mapper.Map<StrategusUserViewModel>(user.StrategusUser));
            }
        }
    }
}
