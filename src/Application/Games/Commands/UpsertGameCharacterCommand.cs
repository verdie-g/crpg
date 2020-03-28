using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    public class UpsertGameCharacterCommand : IRequest<GameCharacter>
    {
        public long SteamId { get; set; } = default!;
        public string CharacterName { get; set; } = default!;

        public class Handler : IRequestHandler<UpsertGameCharacterCommand, GameCharacter>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<GameCharacter> Handle(UpsertGameCharacterCommand request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Where(u => u.SteamId == request.SteamId)
                    // https://github.com/dotnet/efcore/issues/1833#issuecomment-603543685
                    // .Include(u => u.Characters.Where(c => c.Name == request.CharacterName))
                    .Include(u => u.Characters)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    user = new User
                    {
                        SteamId = request.SteamId,
                        Gold = Constants.StartingGold,
                        Role = Constants.DefaultRole,
                        Characters = new List<Character>
                        {
                            new Character
                            {
                                Name = request.CharacterName,
                                Level = 1,
                            }
                        },
                    };

                    _db.Users.Add(user);
                }
                else if (user.Characters.Count == 0)
                {
                    user.Characters.Add(new Character
                    {
                        Name = request.CharacterName,
                        Level = 1,
                    });
                }

                if (_db.Entry(user).State != EntityState.Unchanged
                    || _db.Entry(user.Characters[0]).State != EntityState.Unchanged)
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return _mapper.Map<GameCharacter>(user.Characters[0]);
            }
        }
    }
}