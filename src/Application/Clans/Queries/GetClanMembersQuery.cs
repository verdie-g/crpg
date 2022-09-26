using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries;

public record GetClanMembersQuery : IMediatorRequest<IList<ClanMemberViewModel>>
{
    public int ClanId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetClanMembersQuery, IList<ClanMemberViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ClanMemberViewModel>>> Handle(GetClanMembersQuery req, CancellationToken cancellationToken)
        {
            var clan = await _db.Clans
                .Include(c => c.Members.OrderByDescending(m => m.Role)).ThenInclude(c => c.User)
                .Where(c => c.Id == req.ClanId)
                .FirstOrDefaultAsync(cancellationToken);

            return clan == null
                ? new(CommonErrors.ClanNotFound(req.ClanId))
                : new(_mapper.Map<IList<ClanMemberViewModel>>(clan.Members));
        }
    }
}
