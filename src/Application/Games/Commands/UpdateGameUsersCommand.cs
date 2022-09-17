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

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
        }

        public async Task<Result<UpdateGameUsersResult>> Handle(UpdateGameUsersCommand req,
            CancellationToken cancellationToken)
        {
            var charactersById = await LoadCharacters(req.Updates, cancellationToken);
            List<(User user, GameUserEffectiveReward reward, List<GameUserBrokenItem> brokenItems)> results = new(req.Updates.Count);
            foreach (var update in req.Updates)
            {
                if (!charactersById.TryGetValue(update.CharacterId, out Character? character))
                {
                    Logger.LogWarning("Character with id '{0}' doesn't exist", update.CharacterId);
                    continue;
                }

                var reward = GiveReward(character, update.Reward);
                UpdateStatistics(character, update.Statistics);
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
                    BrokenItems = r.brokenItems,
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
            _characterService.GiveExperience(character, reward.Experience);

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

        private Task<List<GameUserBrokenItem>> RepairOrBreakItems(Character character, IEnumerable<GameUserBrokenItem> itemsToRepair,
            CancellationToken cancellationToken)
        {
            List<GameUserBrokenItem> brokenItems = new();
            foreach (var itemToRepair in itemsToRepair)
            {
                if (character.AutoRepair && character.User!.Gold >= itemToRepair.RepairCost)
                {
                    character.User.Gold -= itemToRepair.RepairCost;
                }

                brokenItems.Add(itemToRepair);
            }

            return Task.FromResult(brokenItems);
            // TODO: downgrade items.
        }

        private async Task<List<GameUserBrokenItem>> DowngradeItems(List<GameUserBrokenItem> brokenItems, CancellationToken cancellationToken)
        {
            int[] userItemIds = brokenItems.Select(bi => bi.UserItemId).ToArray();
            var userItems = await _db.UserItems
                .Where(ui => userItemIds.Contains(ui.Id))
                .ToArrayAsync(cancellationToken);
            foreach (var userItem in userItems)
            {
                if (userItem.Rank > -3)
                {
                    // TODO: check user item with this rank already exist
                    userItem.Rank -= 1;
                }
            }

            return brokenItems;
        }
    }
}
