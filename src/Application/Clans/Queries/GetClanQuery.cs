using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries;

public record GetClanQuery : IMediatorRequest<ClanViewModel>
{
    public int ClanId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetClanQuery, ClanViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<ClanViewModel>> Handle(GetClanQuery req, CancellationToken cancellationToken)
        {
            var clan = await _db.Clans
                .Where(c => c.Id == req.ClanId)
                .ProjectTo<ClanViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return clan == null
                ? new(CommonErrors.ClanNotFound(req.ClanId))
                : new(clan);
        }
    }
}
