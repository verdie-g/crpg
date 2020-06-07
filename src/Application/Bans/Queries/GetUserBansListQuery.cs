using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Bans.Queries
{
    public class GetUserBansListQuery : IRequest<IList<BanViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserBansListQuery, IList<BanViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IList<BanViewModel>> Handle(GetUserBansListQuery request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.Bans).ThenInclude(b => b.BannedByUser)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                return _mapper.Map<BanViewModel[]>(user.Bans);
            }
        }
    }
}