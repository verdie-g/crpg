using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Games.Commands
{
    /// <summary>
    /// Give gold experience and break items of game users.
    /// </summary>
    public class UpdateGameUsersCommand : IMediatorRequest<UpdateGameUsersResult>
    {
        public IList<GameUserUpdate> Updates { get; set; } = Array.Empty<GameUserUpdate>();

        public class Handler : IMediatorRequestHandler<UpdateGameUsersCommand, UpdateGameUsersResult>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ILogger<UpdateGameUsersCommand> _logger;

            public Handler(ICrpgDbContext db, IMapper mapper, ILogger<UpdateGameUsersCommand> logger)
            {
                _db = db;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<UpdateGameUsersResult>> Handle(UpdateGameUsersCommand req,
                CancellationToken cancellationToken)
            {
                int[] characterIds = req.Updates.Select(u => u.CharacterId).ToArray();
                var charactersById = await _db.Characters
                    .Include(c => c.User)
                    .Include(c => c.EquippedItems).ThenInclude(ei => ei.Item)
                    .Where(c => characterIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, cancellationToken);

                var results = new List<(Character character, List<GameUserBrokenItem> brokenItems)>();
                foreach (var update in req.Updates)
                {
                    if (!charactersById.TryGetValue(update.CharacterId, out Character? character))
                    {
                        _logger.LogWarning("Character with id '{0}' doesn't exist", update.CharacterId);
                        continue;
                    }

                    GiveReward(character, update.Reward);
                    var brokenItems = await RepairOrBreakItems(character, update.BrokenItems, cancellationToken);
                    results.Add((character, brokenItems));
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<UpdateGameUsersResult>(new UpdateGameUsersResult
                {
                    UpdateResults = results.Select(r => new UpdateGameUserResult
                    {
                        User = _mapper.Map<GameUser>(r.character.User),
                        BrokenItems = r.brokenItems,
                    }).ToArray(),
                });
            }

            private void GiveReward(Character character, GameUserReward reward)
            {
                character.User!.Gold += reward.Gold;
                character.Experience += (int)(character.ExperienceMultiplier * reward.Experience);
                int newLevel = ExperienceTable.GetLevelForExperience(character.Experience);
                if (character.Level != newLevel) // if user leveled up
                {
                    CharacterHelper.LevelUp(character, newLevel);
                }
            }

            private Task<List<GameUserBrokenItem>> RepairOrBreakItems(Character character, IEnumerable<GameUserBrokenItem> itemsToRepair,
                CancellationToken cancellationToken)
            {
                var brokenItems = new List<GameUserBrokenItem>();
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
                var brokenItemIds = brokenItems.Select(bi => bi.ItemId).ToArray();
                var downrankedItemsByOriginalItemId = await _db.Items
                    .AsNoTracking()
                    .Join(_db.Items, i => i.BaseItemId, i => i.BaseItemId, (i, f) => new { Original = i, Family = f })
                    .Where(o => brokenItemIds.Contains(o.Original.Id) && o.Family.Rank == o.Original.Rank - 1)
                    .ToDictionaryAsync(o => o.Original.Id, o => o.Family, cancellationToken);

                var equippedItemsByItemId = (await _db.EquippedItems
                        .Where(ei => ei.CharacterId == character.Id && brokenItemIds.Contains(ei.ItemId))
                        .ToArrayAsync(cancellationToken))
                    .ToLookup(ei => ei.ItemId);

                foreach (var brokenItemId in brokenItemIds)
                {
                    _db.Entry(new UserItem { UserId = character.UserId, ItemId = brokenItemId }).State = EntityState.Deleted;
                    if (downrankedItemsByOriginalItemId.TryGetValue(brokenItemId, out var downrankedItem))
                    {
                        var downrankedUserItem = new UserItem { UserId = character.UserId, ItemId = downrankedItem.Id };
                        character.User!.OwnedItems.Add(downrankedUserItem);

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
}
