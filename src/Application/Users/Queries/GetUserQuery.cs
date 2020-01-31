using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Common.Mappings;
using Trpg.Application.Steam;
using Trpg.Domain.Entities;

namespace Trpg.Application.Users.Queries
{
    public class GetUserQuery : IRequest<UserModelView>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserQuery, UserModelView>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<UserModelView> Handle(GetUserQuery query, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .ProjectTo<UserModelView>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), query.UserId);
                }

                return user;
            }
        }
    }
}