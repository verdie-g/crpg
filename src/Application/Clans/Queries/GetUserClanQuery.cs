using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries
{
    public record GetUserClanQuery : IMediatorRequest<ClanViewModel>
    {
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetUserClanQuery, ClanViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<ClanViewModel>> Handle(GetUserClanQuery req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .AsNoTracking()
                    .Include(u => u.ClanMembership!.Clan)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new(CommonErrors.UserNotFound(req.UserId));
                }

                return new(_mapper.Map<ClanViewModel>(user.ClanMembership?.Clan));
            }
        }
    }
}
