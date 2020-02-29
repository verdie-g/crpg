using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries
{
    public class GetUserQuery : IRequest<UserViewModel>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserQuery, UserViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<UserViewModel> Handle(GetUserQuery query, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
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