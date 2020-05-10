using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    public class RewardCommand : IRequest<RewardResponse>
    {
        public IList<UserReward> Users { get; set; } = Array.Empty<UserReward>();

        public class Handler : IRequestHandler<RewardCommand, RewardResponse>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<RewardResponse> Handle(RewardCommand request, CancellationToken cancellationToken)
            {
                var rewardByCharacterId = request.Users.ToDictionary(
                    u => u.CharacterId,
                    u => u);

                var dbCharacters = await _db.Characters
                    .Where(c => rewardByCharacterId.ContainsKey(c.Id))
                    .Include(c => c.User)
                    .ToArrayAsync(cancellationToken);

                var userRewardResponses = new List<UserRewardResponse>();
                foreach (var character in dbCharacters)
                {
                    var reward = rewardByCharacterId[character.Id];
                    character.User!.Gold += reward.GoldGain;
                    character.Experience += (int)(reward.ExperienceGain * character.ExperienceMultiplier);
                    int newLevel = ExperienceTable.GetLevelForExperience(character.Experience);
                    if (character.Level != newLevel) // if user leveled up
                    {
                        CharacterHelper.LevelUp(character, newLevel);
                    }

                    userRewardResponses.Add(new UserRewardResponse
                    {
                        UserId = character.UserId,
                        Level = character.Level,
                        Experience = character.Experience,
                        NextLevelExperience = ExperienceTable.GetExperienceForLevel(character.Level + 1),
                        Gold = character.User.Gold,
                    });
                }

                await _db.SaveChangesAsync(cancellationToken);

                return new RewardResponse { Users = userRewardResponses };
            }
        }
    }
}