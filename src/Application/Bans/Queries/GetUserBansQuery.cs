using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Bans.Queries
{
    public record GetUserBansQuery : IMediatorRequest<IList<BanViewModel>>
    {
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetUserBansQuery, IList<BanViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<BanViewModel>>> Handle(GetUserBansQuery request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .AsNoTracking()
                    .Include(u => u.Bans).ThenInclude(b => b.BannedByUser)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                return user == null
                    ? new(CommonErrors.UserNotFound(request.UserId))
                    : new(_mapper.Map<BanViewModel[]>(user.Bans));
            }
        }
    }
}
