﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetLeaderboardQuery : IMediatorRequest<IList<CharacterPublicViewModel>>
{
    public Region? Region { get; set; }

    public CharacterClass? CharacterClass { get; set; }

    internal class Handler : IMediatorRequestHandler<GetLeaderboardQuery, IList<CharacterPublicViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<CharacterPublicViewModel>>> Handle(GetLeaderboardQuery req, CancellationToken cancellationToken)
        {
            var topRatedCharactersByRegion = await _db.Characters
                .OrderByDescending(c => c.Rating.CompetitiveValue)
                .Where(c => (req.Region == null || req.Region == c.User!.Region)
                            && (req.CharacterClass == null || req.CharacterClass == c.Class))
                .Take(50)
                .ProjectTo<CharacterPublicViewModel>(_mapper.ConfigurationProvider)
                .AsSplitQuery()
                .ToArrayAsync();

            return new(topRatedCharactersByRegion);
        }
    }
}
