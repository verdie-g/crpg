using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries
{
    public class GetUserQuery : IMediatorRequest<UserViewModel>
    {
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<GetUserQuery, UserViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<UserViewModel>> Handle(GetUserQuery req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                return user == null
                    ? new Result<UserViewModel>(CommonErrors.UserNotFound(req.UserId))
                    : new Result<UserViewModel>(user);
            }
        }
    }
}
