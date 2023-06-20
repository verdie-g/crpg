using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Games.Commands;

/// <summary>
/// Give gold experience and break items of game users.
/// </summary>
public record UpdateGameUsersCommand : IMediatorRequest<UpdateGameUsersResult>
{
    public IList<GameUserUpdate> Updates { get; init; } = Array.Empty<GameUserUpdate>();

    internal class Handler : IMediatorRequestHandler<UpdateGameUsersCommand, UpdateGameUsersResult>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateGameUsersCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICompetitiveRatingModel _competitiveRatingModel;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService, IActivityLogService activityLogService, ICompetitiveRatingModel competitiveRatingModel)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _activityLogService = activityLogService;
            _competitiveRatingModel = competitiveRatingModel;
        }

        public async Task<Result<UpdateGameUsersResult>> Handle(UpdateGameUsersCommand req,
            CancellationToken cancellationToken)
        {
            var charactersById = await LoadCharacters(req.Updates, cancellationToken);
            List<(User user, GameUserEffectiveReward reward, List<GameRepairedItem> repairedItems)> results = new(req.Updates.Count);
            foreach (var update in req.Updates)
            {
                if (!charactersById.TryGetValue(update.CharacterId, out Character? character))
                {
                    Logger.LogWarning("Character with id '{0}' doesn't exist", update.CharacterId);
                    continue;
                }

                var reward = GiveReward(character, update.Reward);
                UpdateStatistics(character, update.Statistics);
                UpdateRating(character, update.Rating);
                var brokenItems = await RepairOrBreakItems(character, update.BrokenItems, cancellationToken);
                results.Add((character.User!, reward, brokenItems));
            }

            await _db.SaveChangesAsync(cancellationToken);
            return new(new UpdateGameUsersResult
            {
                UpdateResults = results.Select(r => new UpdateGameUserResult
                {
                    User = _mapper.Map<GameUserViewModel>(r.user),
                    EffectiveReward = r.reward,
                    RepairedItems = r.repairedItems,
                }).ToArray(),
            });
        }

        private async Task<Dictionary<int, Character>> LoadCharacters(IList<GameUserUpdate> updates, CancellationToken cancellationToken)
        {
            int[] characterIds = updates.Select(u => u.CharacterId).ToArray();
            var charactersById = await _db.Characters
                .Include(c => c.User!.ClanMembership)
                .Where(c => characterIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

            // Load items in a separate query to avoid cartesian explosion. The items will be automatically set
            // to their respective character.
            await _db.EquippedItems
                .Where(ei => characterIds.Contains(ei.CharacterId))
                .Include(ei => ei.UserItem)
                .LoadAsync(cancellationToken);

            return charactersById;
        }

        private GameUserEffectiveReward GiveReward(Character character, GameUserReward reward)
        {
            int level = character.Level;
            int experience = character.Experience;

            character.User!.Gold += reward.Gold;
            _characterService.GiveExperience(character, reward.Experience, useExperienceMultiplier: true);

            return new GameUserEffectiveReward
            {
                Experience = character.Experience - experience,
                Gold = reward.Gold,
                LevelUp = character.Level != level,
            };
        }

        private void UpdateStatistics(Character character, CharacterStatisticsViewModel statistics)
        {
            character.Statistics.Kills += statistics.Kills;
            character.Statistics.Deaths += statistics.Deaths;
            character.Statistics.Assists += statistics.Assists;
            character.Statistics.PlayTime += statistics.PlayTime;
        }

        private void UpdateRating(Character character, CharacterRatingViewModel rating)
        {
            character.Rating.Value = rating.Value;
            character.Rating.Deviation = rating.Deviation;
            character.Rating.Volatility = rating.Volatility;
            character.Rating.CompetitiveValue = _competitiveRatingModel.ComputeCompetitiveRating(character.Rating);
        }

        private async Task<List<GameRepairedItem>> RepairOrBreakItems(Character character,
            IList<GameUserDamagedItem> damagedItems, CancellationToken cancellationToken)
        {
            List<GameRepairedItem> repairedItems = new();
            List<int> userItemIdsToBreak = new();

            foreach (var damagedItem in damagedItems)
            {
                if (character.User!.Gold >= damagedItem.RepairCost)
                {
                    character.User.Gold -= damagedItem.RepairCost;
                    repairedItems.Add(new GameRepairedItem
                    {
                        ItemId = string.Empty,
                        RepairCost = damagedItem.RepairCost,
                        Broke = false,
                    });
                }
                else
                {
                    userItemIdsToBreak.Add(damagedItem.UserItemId);
                }
            }

            if (userItemIdsToBreak.Count == 0)
            {
                return repairedItems;
            }

            Logger.LogInformation("User '{0}' broke '{1}' items",
                character.UserId, userItemIdsToBreak.Count);

            var userItemsToBreak = await _db.UserItems
                .Where(ui => userItemIdsToBreak.Contains(ui.Id))
                .Include(ui => ui.EquippedItems)
                .ToArrayAsync(cancellationToken);
            foreach (var userItem in userItemsToBreak)
            {
                userItem.IsBroken = true;
                _db.EquippedItems.RemoveRange(userItem.EquippedItems);
                repairedItems.Add(new GameRepairedItem
                {
                    ItemId = userItem.ItemId,
                    RepairCost = 0,
                    Broke = true,
                });
                _db.ActivityLogs.Add(_activityLogService.CreateItemBrokeLog(character.UserId, userItem.ItemId));
            }

            return repairedItems;
        }
    }
}
