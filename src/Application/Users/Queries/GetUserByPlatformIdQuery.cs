using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries;

public record GetUserByPlatformIdQuery : IMediatorRequest<UserPublicViewModel[]>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;

    internal class Handler : IMediatorRequestHandler<GetUserByPlatformIdQuery, UserPublicViewModel[]>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<UserPublicViewModel[]>> Handle(GetUserByPlatformIdQuery req, CancellationToken cancellationToken)
        {
           return new(await _db.Users
                .ProjectTo<UserPublicViewModel>(_mapper.ConfigurationProvider)
                .Where(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId)
                .ToArrayAsync(cancellationToken));
        }
    }
}
