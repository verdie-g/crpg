using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Models;
using Crpg.Common;
using Crpg.Domain.Entities;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    /// <summary>
    /// All in one command to get or update game users.
    /// </summary>
    public class UpdateGameCommand : IRequest<UpdateGameResult>
    {
        public IList<GameUserUpdate> GameUserUpdates { get; set; } = Array.Empty<GameUserUpdate>();

        public class Handler : IRequestHandler<UpdateGameCommand, UpdateGameResult>
        {
            internal static readonly CharacterItems[] DefaultItemSets =
            {
                // aserai
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_wrapped_desert_cap" },
                    BodyItem = new Item { MbId = "mp_aserai_civil_e" },
                    LegItem = new Item { MbId = "mp_strapped_shoes" },
                    Weapon1Item = new Item { MbId = "mp_aserai_axe" },
                    Weapon2Item = new Item { MbId = "mp_throwing_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // vlandia
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_arming_coif" },
                    BodyItem = new Item { MbId = "mp_sackcloth_tunic" },
                    LegItem = new Item { MbId = "mp_strapped_shoes" },
                    Weapon1Item = new Item { MbId = "mp_vlandian_billhook" },
                    Weapon2Item = new Item { MbId = "mp_sling_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // empire
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_laced_cloth_coif" },
                    BodyItem = new Item { MbId = "mp_hemp_tunic" },
                    LegItem = new Item { MbId = "mp_leather_shoes" },
                    Weapon1Item = new Item { MbId = "mp_empire_axe" },
                    Weapon2Item = new Item { MbId = "mp_throwing_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // sturgia
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_nordic_fur_cap" },
                    BodyItem = new Item { MbId = "mp_northern_tunic" },
                    LegItem = new Item { MbId = "mp_wrapped_shoes" },
                    Weapon1Item = new Item { MbId = "mp_sturgia_mace" },
                    Weapon2Item = new Item { MbId = "mp_sling_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // khuzait
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_nomad_padded_hood" },
                    BodyItem = new Item { MbId = "mp_khuzait_civil_coat_b" },
                    LegItem = new Item { MbId = "mp_strapped_leather_boots" },
                    Weapon1Item = new Item { MbId = "mp_khuzait_sichel" },
                    Weapon2Item = new Item { MbId = "mp_throwing_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // battania
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_battania_civil_hood" },
                    BodyItem = new Item { MbId = "mp_battania_civil_a" },
                    LegItem = new Item { MbId = "mp_rough_tied_boots" },
                    Weapon1Item = new Item { MbId = "mp_battania_axe" },
                    Weapon2Item = new Item { MbId = "mp_sling_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
                // looters
                new CharacterItems
                {
                    HeadItem = new Item { MbId = "mp_vlandia_bandit_cape_a" },
                    BodyItem = new Item { MbId = "mp_burlap_sack_dress" },
                    LegItem = new Item { MbId = "mp_strapped_leather_boots" },
                    Weapon1Item = new Item { MbId = "mp_empire_long_twohandedaxe" },
                    Weapon2Item = new Item { MbId = "mp_throwing_stone" },
                    AutoRepair = CharacterHelper.DefaultAutoRepair,
                },
            };

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEventRaiser _events;
            private readonly IDateTimeOffset _dateTime;
            private readonly IRandom _random;

            public Handler(ICrpgDbContext db, IMapper mapper, IEventRaiser events, IDateTimeOffset dateTime, IRandom random)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
                _dateTime = dateTime;
                _random = random;
            }

            public async Task<UpdateGameResult> Handle(UpdateGameCommand cmd, CancellationToken cancellationToken)
            {
                // load default items only once, the first time the command is called
                if (!_db.Entry(DefaultItemSets.First().HeadItem!).IsKeySet)
                {
                    await LoadDefaultItemSets();
                }

                var res = new List<GameUser>();
                var brokenItemsWithUser = new List<(int, GameUserBrokenItem[])>();
                Dictionary<long, User> users = await GetOrCreateUsers(cmd.GameUserUpdates, cancellationToken);
                foreach (GameUserUpdate update in cmd.GameUserUpdates)
                {
                    User user = users[update.SteamId];
                    Character character = user.Characters.FirstOrDefault() ?? CreateCharacterForUser(user, update.CharacterName);

                    if (update.Reward != null)
                    {
                        Reward(character, update.Reward);
                    }

                    var activeBan = GetActiveBan(user.Bans);
                    var brokenItems = RepairOrBreak(character, update.BrokenItems).ToArray();
                    if (brokenItems.Length != 0)
                    {
                        brokenItemsWithUser.Add((user.Id, brokenItems));
                    }

                    res.Add(new GameUser
                    {
                        Id = user.Id,
                        SteamId = user.SteamId,
                        Gold = user.Gold,
                        Character = _mapper.Map<CharacterViewModel>(character),
                        BrokenItems = _mapper.Map<IList<GameUserBrokenItem>>(brokenItems),
                        Ban = _mapper.Map<BanViewModel>(activeBan),
                    });
                }

                await ReplaceBrokenItems(brokenItemsWithUser, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new UpdateGameResult { Users = res };
            }

            private void Reward(Character character, GameUserReward reward)
            {
                character.User!.Gold += reward.Gold;
                character.Experience += (int)(reward.Experience * character.ExperienceMultiplier);
                int newLevel = ExperienceTable.GetLevelForExperience(character.Experience);
                if (character.Level != newLevel) // if user leveled up
                {
                    CharacterHelper.LevelUp(character, newLevel);
                }
            }

            private IEnumerable<GameUserBrokenItem> RepairOrBreak(Character character, IEnumerable<GameUserBrokenItem> brokenItems)
            {
                foreach (var brokenItem in brokenItems)
                {
                    if (character.Items.AutoRepair && character.User!.Gold >= brokenItem.RepairCost)
                    {
                        character.User.Gold -= brokenItem.RepairCost;
                    }
                    else
                    {
                        // Item breaking is only done at the end of the command, after the response was generated,
                        // meaning the user will still be using the unbroken item until this command is called again
                        yield return brokenItem;
                    }
                }
            }

            private Ban? GetActiveBan(IList<Ban> bans)
            {
                Ban? lastBan = bans.OrderByDescending(b => b.Id).FirstOrDefault();
                return lastBan != null && lastBan.CreatedAt + lastBan.Duration > _dateTime.Now ? lastBan : null;
            }

            private async Task<Dictionary<long, User>> GetOrCreateUsers(IList<GameUserUpdate> gameUserUpdates, CancellationToken cancellationToken)
            {
                long[] userSteamIds = gameUserUpdates.Select(u => u.SteamId).ToArray();
                // build predicate to get all users by their steam id and character name
                ExpressionStarter<Character> characterPredicate = PredicateBuilder.New<Character>();
                foreach (var update in gameUserUpdates)
                {
                    characterPredicate = characterPredicate.Or(c =>
                        c.User!.SteamId == update.SteamId && c.Name == update.CharacterName);
                }

                Expression<Func<Character, bool>> characterPredicateExpr = characterPredicate; // https://stackoverflow.com/a/46716258/5407910

                static User ElemSelector(Tuple<User, Character> p)
                {
                    if (p.Item2 != null)
                    {
                        p.Item1.Characters = new List<Character> { p.Item2 };
                    }

                    return p.Item1;
                }

                var users = await _db.Users
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HeadItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.BodyItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.CapeItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HandItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.LegItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HorseHarnessItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HorseItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon1Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon2Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon3Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon4Item)
                    .Include(u => u.Bans) // could be filtered https://github.com/dotnet/efcore/issues/1833#issuecomment-603543685
                    .AsExpandable()
                    .Where(u => userSteamIds.Contains(u.SteamId))
                    .Select(u => new Tuple<User, Character>(u, u.Characters.FirstOrDefault(characterPredicateExpr.Compile())))
                    .ToDictionaryAsync(p => p.Item1.SteamId, ElemSelector, cancellationToken);

                if (gameUserUpdates.Count == users.Count) // if all users already exist
                {
                    return users;
                }

                // else some users don't exist, we need to create them and their first character
                foreach (var update in gameUserUpdates)
                {
                    if (users.ContainsKey(update.SteamId))
                    {
                        continue;
                    }

                    var user = CreateUser(update.SteamId, update.CharacterName);

                    users[user.SteamId] = user;
                    _events.Raise(EventLevel.Info, $"{update.CharacterName} joined ({update.SteamId})", string.Empty, "user_created");
                }

                return users;
            }

            private User CreateUser(long steamId, string characterName)
            {
                var user = new User { SteamId = steamId };
                UserHelper.SetDefaultValuesForUser(user);
                CreateCharacterForUser(user, characterName);
                return user;
            }

            private Character CreateCharacterForUser(User user, string characterName)
            {
                var character = CreateCharacter(characterName);
                character.User = user;
                _db.Characters.Add(character);

                foreach (var (_, item) in character.Items.ItemSlotPairs())
                {
                    // Add character items to user inventory
                    user.OwnedItems.Add(new UserItem { ItemId = item.Id });

                    // Detach character items to avoid adding them when they already exist (dirty hack)
                    _db.Entry(item).State = EntityState.Detached;
                }

                return character;
            }

            private Character CreateCharacter(string name)
            {
                var character = new Character
                {
                    Name = name,
                    Generation = CharacterHelper.DefaultGeneration,
                    Level = CharacterHelper.DefaultLevel,
                    Experience = CharacterHelper.DefaultExperience,
                    ExperienceMultiplier = CharacterHelper.DefaultExperienceMultiplier,
                    Items = DefaultItemSets[_random.Next(DefaultItemSets.Length - 1)],
                };
                CharacterHelper.ResetCharacterStats(character);

                return character;
            }

            private async Task ReplaceBrokenItems(List<(int userId, GameUserBrokenItem[] brokenItems)> brokenItemsWithUser,
                CancellationToken cancellationToken)
            {
                if (brokenItemsWithUser.Count == 0)
                {
                    return;
                }

                var brokenItemIds = brokenItemsWithUser
                    .SelectMany(bi => bi.brokenItems.Select(b => b.ItemId))
                    .Distinct()
                    .ToArray();

                var downrankedItemsByOriginalItemId = await _db.Items
                    .AsNoTracking()
                    .Join(_db.Items, i => i.BaseItemId, i => i.BaseItemId, (i, f) => new { Original = i, Family = f })
                    .Where(o => brokenItemIds.Contains(o.Original.Id) && o.Family.Rank == o.Original.Rank - 1)
                    .ToDictionaryAsync(o => o.Original.Id, o => o.Family.Id, cancellationToken);

                // query again users but with all their characters this time
                var users = await _db.Users.Include(u => u.Characters).ToDictionaryAsync(u => u.Id, cancellationToken);
                foreach (var (userId, brokenItems) in brokenItemsWithUser)
                {
                    User user = users[userId];
                    foreach (var brokenItem in brokenItems)
                    {
                        int? downrankedItemId = downrankedItemsByOriginalItemId.TryGetValue(brokenItem.ItemId, out int tmp)
                            ? tmp
                            : (int?)null; // the query did not return the downranked item, meaning the item was destroyed

                        user.OwnedItems.Remove(new UserItem { ItemId = brokenItem.ItemId });
                        if (downrankedItemId != null)
                        {
                            user.OwnedItems.Add(new UserItem { ItemId = downrankedItemId.Value });
                        }

                        foreach (Character character in user.Characters)
                        {
                            CharacterHelper.ReplaceCharacterItem(character.Items, brokenItem.ItemId, downrankedItemId);
                        }
                    }
                }
            }

            private async Task LoadDefaultItemSets()
            {
                ExpressionStarter<Item> itemPredicate = PredicateBuilder.New<Item>();
                foreach (CharacterItems itemSet in DefaultItemSets)
                {
                    foreach ((_, Item item) in itemSet.ItemSlotPairs())
                    {
                        itemPredicate = itemPredicate.Or(i => i.MbId == item.MbId);
                    }
                }

                Dictionary<string, Item> itemsByMbId = await _db.Items
                    .AsNoTracking()
                    .Where(itemPredicate)
                    .ToDictionaryAsync(i => i.MbId, i => i);

                foreach (CharacterItems itemSet in DefaultItemSets)
                {
                    itemSet.HeadItem = itemsByMbId[itemSet.HeadItem!.MbId];
                    itemSet.BodyItem = itemsByMbId[itemSet.BodyItem!.MbId];
                    itemSet.LegItem = itemsByMbId[itemSet.LegItem!.MbId];
                    itemSet.Weapon1Item = itemsByMbId[itemSet.Weapon1Item!.MbId];
                    itemSet.Weapon2Item = itemsByMbId[itemSet.Weapon2Item!.MbId];
                }
            }
        }
    }
}
