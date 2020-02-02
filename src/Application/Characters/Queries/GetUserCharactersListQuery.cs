using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Common.Validators;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters.Queries
{
    public class GetUserCharactersListQuery : IRequest<IReadOnlyList<CharacterModelView>>
    {
        public int UserId { get; set; }

        public class Validator : AbstractValidator<GetUserCharactersListQuery>
        {
            public Validator(ITrpgDbContext db)
            {
                Include(new KeyValidator<GetUserCharactersListQuery, User, int>(db.Users, c => c.UserId));
            }
        }

        public class Handler : IRequestHandler<GetUserCharactersListQuery, IReadOnlyList<CharacterModelView>>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<CharacterModelView>> Handle(GetUserCharactersListQuery request, CancellationToken cancellationToken)
            {
                return await _db.Characters
                    .Where(c => c.UserId == request.UserId)
                    .ProjectTo<CharacterModelView>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
