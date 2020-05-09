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
            private const int AttributePointsPerLevel = 1;
            private const int SkillPointsPerLevel = 1;

            private static int WeaponProficiencyPointsForLevel(int lvl)
            {
                const int a = 1;
                const int b = 49;
                const int c = 52;

                return (int)(a * Math.Pow(lvl, 2) + b * lvl + c);
            }

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
                    character.Experience += (int)(tick.ExperienceGain * character.ExperienceMultiplier);
                    int newLevel = ExperienceTable.GetLevelForExperience(character.Experience);
                    if (character.Level != newLevel) // if user leveled up
                    {
                        int levelDiff = newLevel - character.Level;
                        character.Statistics.Attributes.Points += levelDiff * AttributePointsPerLevel;
                        character.Statistics.Skills.Points += levelDiff * SkillPointsPerLevel;
                        character.Statistics.WeaponProficiencies.Points += WeaponProficiencyPointsForLevel(newLevel)
                            - WeaponProficiencyPointsForLevel(character.Level);
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