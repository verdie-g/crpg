using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    public class TickCommand : IRequest<TickResponse>
    {
        public IReadOnlyList<UserTick> Users { get; set; } = Array.Empty<UserTick>();

        public class Handler : IRequestHandler<TickCommand, TickResponse>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<TickResponse> Handle(TickCommand request, CancellationToken cancellationToken)
            {
                var tickByCharacterId = request.Users.ToDictionary(
                    u => u.CharacterId,
                    u => u);

                var dbCharacters = await _db.Characters
                    .Where(c => tickByCharacterId.ContainsKey(c.Id))
                    .Include(c => c.User)
                    .ToArrayAsync(cancellationToken);

                var tickUserResponse = new List<TickUserResponse>();
                foreach (var character in dbCharacters)
                {
                    var tick = tickByCharacterId[character.Id];
                    character.User!.Gold += tick.GoldGain;
                    character.Experience += tick.ExperienceGain;
                    int newLevel = ExperienceTable.GetLevelForExperience(character.Experience);
                    if (character.Level != newLevel) // if user leveled up
                    {
                        character.Level = newLevel;
                        tickUserResponse.Add(new TickUserResponse
                        {
                            UserId = character.UserId,
                            Level = newLevel,
                            NextLevelExperience = ExperienceTable.GetExperienceForLevel(newLevel + 1),
                        });
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);

                return new TickResponse { Users = tickUserResponse };
            }
        }
    }
}