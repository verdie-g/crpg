﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries;

public record GetUserByIdQuery : IMediatorRequest<UserPublicViewModel>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserByIdQuery, UserPublicViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<UserPublicViewModel>> Handle(GetUserByIdQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .ProjectTo<UserPublicViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            return user == null
                ? new(CommonErrors.UserNotFound(req.UserId))
                : new(user);
        }
    }
}
