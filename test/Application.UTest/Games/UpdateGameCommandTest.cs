using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Common;
using Crpg.Domain.Entities;
using Crpg.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class UpdateGameCommandTest : TestBase
    {
        [SetUp]
        public override async Task SetUp()
        {
            await base.SetUp();

            var allDefaultItemMbIds = UpdateGameCommand.Handler.DefaultItemSets
                .SelectMany(s => s.ItemSlotPairs())
                .GroupBy(p => p.item.MbId) // distinct by mbId
                .Select(p => new Item { MbId = p.First().item.MbId });

            ArrangeDb.Items.AddRange(allDefaultItemMbIds);
            await ArrangeDb.SaveChangesAsync();
        }

        [Test]
        public void ShouldDoNothingForEmptyUpdates()
        {
            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());
            Assert.DoesNotThrowAsync(() => handler.Handle(new UpdateGameCommand(), CancellationToken.None));
        }

        [Test]
        public async Task ShouldCreateUserIfDoesntExist()
        {
            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "a",
                        Reward = new GameUserReward
                        {
                            Experience = 100,
                            Gold = 200,
                        }
                    },
                }
            }, CancellationToken.None);

            Assert.AreEqual(1, res.Users.Count);
            Assert.AreEqual(300 + 200, res.Users[0].Gold);
            Assert.AreEqual("a", res.Users[0].Character.Name);
            Assert.AreEqual(0, res.Users[0].Character.Generation);
            Assert.AreEqual(1, res.Users[0].Character.Level);
            Assert.AreEqual(100, res.Users[0].Character.Experience);
            Assert.AreEqual(3, res.Users[0].Character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, res.Users[0].Character.Statistics.Attributes.Agility);
            Assert.AreEqual(0, res.Users[0].Character.Statistics.Attributes.Points);
            Assert.AreEqual(0, res.Users[0].Character.Statistics.Skills.Points);
            Assert.Greater(res.Users[0].Character.Statistics.WeaponProficiencies.Points, 0);
            Assert.IsNotNull(res.Users[0].Character.Items.HeadItem);
            Assert.IsNotNull(res.Users[0].Character.Items.BodyItem);
            Assert.IsNotNull(res.Users[0].Character.Items.LegItem);
            Assert.IsNotNull(res.Users[0].Character.Items.Weapon1Item);
            Assert.IsNotNull(res.Users[0].Character.Items.Weapon2Item);
            Assert.IsTrue(res.Users[0].Character.Items.AutoRepair);
            Assert.AreEqual(0, res.Users[0].BrokenItems.Count);
            Assert.IsNull(res.Users[0].Ban);

            // Check that user and its owned entities were created
            var user = await AssertDb.Users
                .Include(u => u.Characters)
                .FirstOrDefaultAsync(u => u.Id == res.Users[0].Id);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Characters);
            Assert.IsNotNull(user.Characters[0].Items.BodyItemId);
            Assert.NotZero(user.Characters[0].Statistics.Attributes.Agility);
        }

        [Test]
        public async Task ShouldCreateCharacterIfDoesntExist()
        {
            var user = new User { PlatformId = "1", Gold = 1000 };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "a",
                        Reward = new GameUserReward
                        {
                            Experience = 100,
                            Gold = 200,
                        }
                    },
                }
            }, CancellationToken.None);

            Assert.AreEqual(1, res.Users.Count);
            Assert.AreEqual(1000 + 200, res.Users[0].Gold);
            Assert.AreEqual("a", res.Users[0].Character.Name);
            Assert.AreEqual(0, res.Users[0].Character.Generation);
            Assert.AreEqual(1, res.Users[0].Character.Level);
            Assert.AreEqual(100, res.Users[0].Character.Experience);
            Assert.NotNull(res.Users[0].Character.Items.HeadItem);
            Assert.NotNull(res.Users[0].Character.Items.BodyItem);
            Assert.NotNull(res.Users[0].Character.Items.LegItem);
            Assert.NotNull(res.Users[0].Character.Items.Weapon1Item);
            Assert.NotNull(res.Users[0].Character.Items.Weapon2Item);
            Assert.IsTrue(res.Users[0].Character.Items.AutoRepair);
            Assert.AreEqual(0, res.Users[0].BrokenItems.Count);
            Assert.IsNull(res.Users[0].Ban);

            // Check that character and its owned entities were created
            user = await AssertDb.Users
                .Include(u => u.Characters)
                .FirstOrDefaultAsync(u => u.Id == res.Users[0].Id);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Characters);
            Assert.IsNotNull(user.Characters[0].Items.BodyItemId);
            Assert.NotZero(user.Characters[0].Statistics.Attributes.Agility);
        }

        [Test]
        public async Task ShouldUpdateExistingCharacterCorrectly()
        {
            var user = new User
            {
                PlatformId = "1",
                Gold = 1000,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "a",
                        Experience = 100,
                        ExperienceMultiplier = 1.0f,
                        Level = 1,
                        Items = new CharacterItems
                        {
                            BodyItem = new Item(),
                        },
                    },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "a",
                        Reward = new GameUserReward
                        {
                            Experience = 300,
                            Gold = 200,
                        }
                    },
                }
            }, CancellationToken.None);

            Assert.AreEqual(1, res.Users.Count);
            Assert.AreEqual(user.Id, res.Users[0].Id);
            Assert.AreEqual(1000 + 200, res.Users[0].Gold);
            Assert.AreEqual("a", res.Users[0].Character.Name);
            Assert.AreEqual(0, res.Users[0].Character.Generation);
            Assert.AreEqual(1, res.Users[0].Character.Level);
            Assert.AreEqual(100 + 300, res.Users[0].Character.Experience);
            Assert.Null(res.Users[0].Character.Items.HeadItem);
            Assert.NotNull(res.Users[0].Character.Items.BodyItem);
            Assert.Null(res.Users[0].Character.Items.LegItem);
            Assert.Null(res.Users[0].Character.Items.Weapon1Item);
            Assert.Null(res.Users[0].Character.Items.Weapon2Item);
            Assert.AreEqual(0, res.Users[0].BrokenItems.Count);
            Assert.IsNull(res.Users[0].Ban);
        }

        [Test]
        public async Task ShouldLevelUpIfEnoughExperience()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                PlatformId = "1",
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "a",
                        ExperienceMultiplier = 2f,
                        Level = 1,
                        Experience = 1000,
                    },
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "a",
                        Reward = new GameUserReward { Experience = 1000 },
                    },
                }
            }, CancellationToken.None);

            Assert.AreEqual(0, res.Users[0].Character.Generation);
            Assert.AreEqual(2, res.Users[0].Character.Level);
            Assert.AreEqual(1000 + 2 * 1000, res.Users[0].Character.Experience);
            Assert.AreEqual(1, res.Users[0].Character.Statistics.Attributes.Points);
            Assert.AreEqual(1, res.Users[0].Character.Statistics.Skills.Points);
            Assert.Greater(res.Users[0].Character.Statistics.WeaponProficiencies.Points, 1);
        }

        [Test]
        public async Task AllInOne()
        {
            var user0 = new User
            {
                PlatformId = "1",
                Gold = 10,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "a",
                        ExperienceMultiplier = 1f,
                        Level = 1,
                        Experience = 100,
                    },
                },
            };

            var user1 = new User
            {
                PlatformId = "2",
                Gold = 20,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "a",
                        ExperienceMultiplier = 1f,
                        Level = 1,
                        Experience = 200,
                    },
                },
            };

            var user2 = new User
            {
                PlatformId = "3",
                Gold = 30,
            };

            ArrangeDb.AddRange(user0, user1, user2);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = user0.PlatformId,
                        CharacterName = user0.Characters[0].Name,
                        Reward = new GameUserReward { Experience = 900, Gold = 90 },
                    },
                    new GameUserUpdate
                    {
                        PlatformId = user1.PlatformId,
                        CharacterName = user1.Characters[0].Name,
                        Reward = new GameUserReward { Experience = 800, Gold = 80 },
                    },
                    new GameUserUpdate
                    {
                        PlatformId = user2.PlatformId,
                        CharacterName = "c",
                        Reward = new GameUserReward { Experience = 1000, Gold = 70 },
                    },
                    new GameUserUpdate
                    {
                        PlatformId = "4",
                        CharacterName = "d",
                        Reward = new GameUserReward { Experience = 1000, Gold = -200 },
                    },
                }
            }, CancellationToken.None);

            Assert.AreEqual(4, res.Users.Count);

            Assert.AreEqual(user0.PlatformId, res.Users[0].PlatformId);
            Assert.AreEqual(user0.Characters[0].Name, res.Users[0].Character.Name);

            Assert.AreEqual(user1.PlatformId, res.Users[1].PlatformId);
            Assert.AreEqual(user1.Characters[0].Name, res.Users[1].Character.Name);

            Assert.AreEqual(user2.PlatformId, res.Users[2].PlatformId);
            Assert.AreEqual("c", res.Users[2].Character.Name);

            Assert.AreEqual("4", res.Users[3].PlatformId);
            Assert.AreEqual("d", res.Users[3].Character.Name);

            foreach (var user in res.Users)
            {
                Assert.AreEqual(100, user.Gold);
                Assert.AreEqual(1000, user.Character.Experience);
            }
        }

        [Test]
        public async Task BanShouldntBeNullForBannedUser()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                PlatformId = "1",
                Bans = new List<Ban>
                {
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                        Duration = TimeSpan.FromDays(1),
                    }
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime
                .Setup(dt => dt.Now)
                .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                dateTime.Object, new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[] { new GameUserUpdate { PlatformId = "1" } }
            }, CancellationToken.None);

            Assert.NotNull(res.Users[0].Ban);
        }

        [Test]
        public async Task BanShouldBeNullForUnbannedUser()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                PlatformId = "1",
                Bans = new List<Ban>
                {
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                        Duration = TimeSpan.FromDays(1),
                    },
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1, 6, 0, 0)),
                        Duration = TimeSpan.Zero,
                    }
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime
                .Setup(dt => dt.Now)
                .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                dateTime.Object, new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[] { new GameUserUpdate { PlatformId = "1" } }
            }, CancellationToken.None);

            Assert.Null(res.Users[0].Ban);
        }

        [Test]
        public async Task BreakingAllCharacterItemsWithAutoRepairShouldRepairThemIfEnoughGold()
        {
            var user = new User
            {
                PlatformId = "1",
                Gold = 10000,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "b",
                        Items = new CharacterItems
                        {
                            HeadItem = new Item(),
                            CapeItem = new Item(),
                            BodyItem = new Item(),
                            HandItem = new Item(),
                            LegItem = new Item(),
                            HorseHarnessItem = new Item(),
                            HorseItem = new Item(),
                            Weapon1Item = new Item(),
                            Weapon2Item = new Item(),
                            Weapon3Item = new Item(),
                            Weapon4Item = new Item(),
                            AutoRepair = true,
                        },
                    },
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "b",
                        BrokenItems = new List<GameUserBrokenItem>
                        {
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HeadItemId!.Value, RepairCost = 100 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.CapeItemId!.Value, RepairCost = 150 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.BodyItemId!.Value, RepairCost = 200 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HandItemId!.Value, RepairCost = 250 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.LegItemId!.Value, RepairCost = 300 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HorseHarnessItemId!.Value, RepairCost = 350 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HorseItemId!.Value, RepairCost = 400 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon1ItemId!.Value, RepairCost = 450 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon2ItemId!.Value, RepairCost = 500 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon3ItemId!.Value, RepairCost = 550 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon4ItemId!.Value, RepairCost = 600 },
                        }
                    }
                }
            }, CancellationToken.None);

            Assert.AreEqual(10000 - 3850, res.Users[0].Gold);
            Assert.AreEqual(0, res.Users[0].BrokenItems.Count);

            var expectedItems = user.Characters[0].Items;
            var actualItems = res.Users[0].Character.Items;
            Assert.AreEqual(expectedItems.HeadItem!.Id, actualItems.HeadItem!.Id);
            Assert.AreEqual(expectedItems.CapeItem!.Id, actualItems.CapeItem!.Id);
            Assert.AreEqual(expectedItems.BodyItem!.Id, actualItems.BodyItem!.Id);
            Assert.AreEqual(expectedItems.HandItem!.Id, actualItems.HandItem!.Id);
            Assert.AreEqual(expectedItems.LegItem!.Id, actualItems.LegItem!.Id);
            Assert.AreEqual(expectedItems.HorseHarnessItem!.Id, actualItems.HorseHarnessItem!.Id);
            Assert.AreEqual(expectedItems.HorseItem!.Id, actualItems.HorseItem!.Id);
            Assert.AreEqual(expectedItems.Weapon1Item!.Id, actualItems.Weapon1Item!.Id);
            Assert.AreEqual(expectedItems.Weapon2Item!.Id, actualItems.Weapon2Item!.Id);
            Assert.AreEqual(expectedItems.Weapon3Item!.Id, actualItems.Weapon3Item!.Id);
            Assert.AreEqual(expectedItems.Weapon4Item!.Id, actualItems.Weapon4Item!.Id);
        }

        [Test]
        public async Task BreakingAllCharacterItemsWithoutAutoRepairShouldBreakThem()
        {
            var user = new User
            {
                PlatformId = "1",
                Gold = 10000,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "b",
                        Items = new CharacterItems
                        {
                            HeadItem = new Item { Rank = 3 },
                            CapeItem = new Item { Rank = 2 },
                            BodyItem = new Item { Rank = 1 },
                            HandItem = new Item { Rank = 0 },
                            LegItem = new Item { Rank = -1 },
                            HorseHarnessItem = new Item { Rank = -2 },
                            HorseItem = new Item { Rank = -3 },
                            Weapon1Item = new Item { Rank = -2 },
                            Weapon2Item = new Item { Rank = -1 },
                            Weapon3Item = new Item { Rank = 0 },
                            Weapon4Item = new Item { Rank = 1 },
                            AutoRepair = false,
                        },
                    },
                }
            };
            ArrangeDb.Users.Add(user);

            // for each item, add its base item (rank = 0) and downranked item (rank = rank - 1)
            foreach (var (_, item) in user.Characters[0].Items.ItemSlotPairs())
            {
                ArrangeDb.UserItems.Add(new UserItem { User = user, Item = item });

                var baseItem = item.Rank == 0 ? item : new Item { Rank = 0 };
                baseItem.BaseItem = baseItem;
                item.BaseItem = baseItem;
                ArrangeDb.Items.Add(baseItem);

                if (item.Rank > -3 && item.Rank - 1 != 0)
                {
                    ArrangeDb.Items.Add(new Item { Rank = item.Rank - 1, BaseItem = baseItem });
                }
            }

            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "b",
                        BrokenItems = new List<GameUserBrokenItem>
                        {
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HeadItemId!.Value, RepairCost = 100 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.CapeItemId!.Value, RepairCost = 150 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.BodyItemId!.Value, RepairCost = 200 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HandItemId!.Value, RepairCost = 250 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.LegItemId!.Value, RepairCost = 300 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HorseHarnessItemId!.Value, RepairCost = 350 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.HorseItemId!.Value, RepairCost = 400 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon1ItemId!.Value, RepairCost = 450 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon2ItemId!.Value, RepairCost = 500 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon3ItemId!.Value, RepairCost = 550 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].Items.Weapon4ItemId!.Value, RepairCost = 600 },
                        }
                    }
                }
            }, CancellationToken.None);

            Assert.AreEqual(10000, res.Users[0].Gold);
            Assert.AreEqual(11, res.Users[0].BrokenItems.Count);

            user = await AssertDb.Users
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
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(2, user.Characters[0].Items.HeadItem!.Rank);
            Assert.AreEqual(1, user.Characters[0].Items.CapeItem!.Rank);
            Assert.AreEqual(0, user.Characters[0].Items.BodyItem!.Rank);
            Assert.AreEqual(-1, user.Characters[0].Items.HandItem!.Rank);
            Assert.AreEqual(-2, user.Characters[0].Items.LegItem!.Rank);
            Assert.AreEqual(-3, user.Characters[0].Items.HorseHarnessItem!.Rank);
            Assert.IsNull(user.Characters[0].Items.HorseItem);
            Assert.AreEqual(-3, user.Characters[0].Items.Weapon1Item!.Rank);
            Assert.AreEqual(-2, user.Characters[0].Items.Weapon2Item!.Rank);
            Assert.AreEqual(-1, user.Characters[0].Items.Weapon3Item!.Rank);
            Assert.AreEqual(0, user.Characters[0].Items.Weapon4Item!.Rank);

            // check broken items were added to user inventory
            foreach (var (_, item) in user.Characters[0].Items.ItemSlotPairs())
            {
                Assert.DoesNotThrowAsync(() => AssertDb.UserItems.FirstAsync(oi => oi.ItemId == item.Id && oi.UserId == user.Id));
            }
        }

        [Test]
        public async Task BreakingCharacterItemsWithAutoRepairShouldRepairUntilThereIsNotEnoughGold()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                PlatformId = "1",
                Gold = 3000,
                Characters = new List<Character>
                {
                    new Character
                    {
                        Name = "b",
                        Items = new CharacterItems
                        {
                            HeadItem = new Item(),
                            CapeItem = new Item(),
                            BodyItem = new Item(),
                            HandItem = new Item(),
                            LegItem = new Item(),
                            AutoRepair = true,
                        },
                    },
                }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateGameCommand.Handler(ActDb, Mapper, Mock.Of<IEventRaiser>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom());

            var res = await handler.Handle(new UpdateGameCommand
            {
                GameUserUpdates = new[]
                {
                    new GameUserUpdate
                    {
                        PlatformId = "1",
                        CharacterName = "b",
                        BrokenItems = new List<GameUserBrokenItem>
                        {
                            new GameUserBrokenItem { ItemId = user.Entity.Characters[0].Items.HeadItemId!.Value, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Entity.Characters[0].Items.CapeItemId!.Value, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Entity.Characters[0].Items.BodyItemId!.Value, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Entity.Characters[0].Items.HandItemId!.Value, RepairCost = 1000 },
                        }
                    }
                }
            }, CancellationToken.None);

            Assert.AreEqual(0, res.Users[0].Gold);
            Assert.AreEqual(1, res.Users[0].BrokenItems.Count); // hand
        }
    }
}
