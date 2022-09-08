﻿using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
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
                var brokenItems = await RepairOrBreakItems(character, update.BrokenItems, cancellationToken);
                results.Add((character.User!, reward, brokenItems));
            }

            await _db.SaveChangesAsync(cancellationToken);
            return new(new UpdateGameUsersResult
            {
                UpdateResults = results.Select(r => new UpdateGameUserResult
                {
                    User = _mapper.Map<GameUser>(r.user),
                    EffectiveReward = r.reward,
                    BrokenItems = r.brokenItems,
                }).ToArray(),
            });
        }

        private async Task<Dictionary<int, Character>> LoadCharacters(IList<GameUserUpdate> updates, CancellationToken cancellationToken)
        {
            int[] characterIds = updates.Select(u => u.CharacterId).ToArray();
            var charactersById = await _db.Characters
                .Include(c => c.User)
                .Where(c => characterIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

            // Load items in a separate query to avoid cartesian explosion. The items will be automatically set
            // to their respective character.
            await _db.EquippedItems
                .Where(ei => characterIds.Contains(ei.CharacterId))
                .Include(ei => ei.Item)
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

        private Task<List<GameUserBrokenItem>> RepairOrBreakItems(Character character, IEnumerable<GameUserBrokenItem> itemsToRepair,
            CancellationToken cancellationToken)
        {
            List<GameUserBrokenItem> brokenItems = new();
            foreach (var itemToRepair in itemsToRepair)
            {
                if (character.AutoRepair && character.User!.Gold >= itemToRepair.RepairCost)
                {
                    character.User.Gold -= itemToRepair.RepairCost;
                    continue;
                }

                brokenItems.Add(itemToRepair);
            }

            return brokenItems.Count == 0
                ? Task.FromResult(brokenItems)
                : DowngradeItems(character, brokenItems, cancellationToken);
        }

        private async Task<List<GameUserBrokenItem>> DowngradeItems(Character character, List<GameUserBrokenItem> brokenItems, CancellationToken cancellationToken)
        {
            int[] brokenItemIds = brokenItems.Select(bi => bi.ItemId).ToArray();
            var downrankedItemsByOriginalItemId = await _db.Items
                .AsNoTracking()
                .Join(_db.Items, i => i.BaseItemId, i => i.BaseItemId, (i, f) => new { Original = i, Family = f })
                .Where(o => brokenItemIds.Contains(o.Original.Id) && o.Family.Rank == o.Original.Rank - 1)
                .ToDictionaryAsync(o => o.Original.Id, o => o.Family, cancellationToken);

            var equippedItemsByItemId = (await _db.EquippedItems
                    .Where(ei => ei.CharacterId == character.Id && brokenItemIds.Contains(ei.ItemId))
                    .ToArrayAsync(cancellationToken))
                .ToLookup(ei => ei.ItemId);

            foreach (int brokenItemId in brokenItemIds)
            {
                _db.Entry(new UserItem { UserId = character.UserId, ItemId = brokenItemId }).State = EntityState.Deleted;
                if (downrankedItemsByOriginalItemId.TryGetValue(brokenItemId, out var downrankedItem))
                {
                    UserItem downrankedUserItem = new() { UserId = character.UserId, ItemId = downrankedItem.Id };
                    character.User!.Items.Add(downrankedUserItem);

                    foreach (var equippedItems in equippedItemsByItemId[brokenItemId])
                    {
                        equippedItems.UserItem = downrankedUserItem;
                    }
                }
                else
                {
                    _db.EquippedItems.RemoveRange(equippedItemsByItemId[brokenItemId]);
                }
            }

            return brokenItems;
        }
    }
}
