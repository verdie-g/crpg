using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class UpdateGameUsersCommandTest : TestBase
    {
        [Test]
        public void ShouldDoNothingForEmptyUpdates()
        {
            Mock<ICharacterService> characterServiceMock = new();
            UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object);
            Assert.DoesNotThrowAsync(() => handler.Handle(new UpdateGameUsersCommand(), CancellationToken.None));
        }

        [Test]
        public async Task ShouldUpdateExistingCharacterCorrectly()
        {
            User user = new()
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                Gold = 1000,
                Characters = new List<Character>
                {
                    new()
                    {
                        Name = "a",
                        Experience = 0,
                        ExperienceMultiplier = 1.0f,
                        Level = 1,
                        EquippedItems = { new EquippedItem { Item = new Item(), Slot = ItemSlot.Body } },
                    },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            Mock<ICharacterService> characterServiceMock = new();
            UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object);
            var result = await handler.Handle(new UpdateGameUsersCommand
            {
                Updates = new[]
                {
                    new GameUserUpdate
                    {
                        CharacterId = user.Characters[0].Id,
                        Reward = new GameUserReward
                        {
                            Experience = 10,
                            Gold = 200,
                        },
                    },
                },
            }, CancellationToken.None);

            var data = result.Data!;
            Assert.AreEqual(1, data.UpdateResults.Count);
            Assert.AreEqual(user.Id, data.UpdateResults[0].User.Id);
            Assert.AreEqual(Platform.Steam, data.UpdateResults[0].User.Platform);
            Assert.AreEqual("1", data.UpdateResults[0].User.PlatformUserId);
            Assert.AreEqual(1000 + 200, data.UpdateResults[0].User.Gold);
            Assert.AreEqual("a", data.UpdateResults[0].User.Character.Name);
            Assert.AreEqual(1, data.UpdateResults[0].User.Character.EquippedItems.Count);
            Assert.IsEmpty(data.UpdateResults[0].BrokenItems);
            Assert.IsNull(data.UpdateResults[0].User.Ban);

            characterServiceMock.Verify(cs => cs.GiveExperience(It.IsAny<Character>(), 10));
        }

        [Test]
        public async Task BreakingAllCharacterItemsWithAutoRepairShouldRepairThemIfEnoughGold()
        {
            User user = new()
            {
                Gold = 10000,
                Characters = new List<Character>
                {
                    new()
                    {
                        Name = "b",
                        EquippedItems =
                        {
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Head },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Shoulder },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Body },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Hand },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Leg },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.MountHarness },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Mount },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon0 },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon1 },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon2 },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon3 },
                        },
                        AutoRepair = true,
                    },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            Mock<ICharacterService> characterServiceMock = new();
            UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object);
            var result = await handler.Handle(new UpdateGameUsersCommand
            {
                Updates = new[]
                {
                    new GameUserUpdate
                    {
                        CharacterId = user.Characters[0].Id,
                        BrokenItems = new[]
                        {
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[0].ItemId, RepairCost = 100 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[1].ItemId, RepairCost = 150 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[2].ItemId, RepairCost = 200 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[3].ItemId, RepairCost = 250 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[4].ItemId, RepairCost = 300 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[5].ItemId, RepairCost = 350 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[6].ItemId, RepairCost = 400 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[7].ItemId, RepairCost = 450 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[8].ItemId, RepairCost = 500 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[9].ItemId, RepairCost = 550 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[10].ItemId, RepairCost = 600 },
                        },
                    },
                },
            }, CancellationToken.None);

            var data = result.Data!;
            Assert.AreEqual(10000 - 3850, data.UpdateResults[0].User.Gold);
            Assert.AreEqual(0, data.UpdateResults[0].BrokenItems.Count);

            var expectedItemsBySlot = user.Characters[0].EquippedItems.ToDictionary(ei => ei.Slot);
            var actualItemsBySlot = data.UpdateResults[0].User.Character.EquippedItems.ToDictionary(ei => ei.Slot);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Head].ItemId, actualItemsBySlot[ItemSlot.Head].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Shoulder].ItemId, actualItemsBySlot[ItemSlot.Shoulder].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Body].ItemId, actualItemsBySlot[ItemSlot.Body].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Hand].ItemId, actualItemsBySlot[ItemSlot.Hand].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Leg].ItemId, actualItemsBySlot[ItemSlot.Leg].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.MountHarness].ItemId, actualItemsBySlot[ItemSlot.MountHarness].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Mount].ItemId, actualItemsBySlot[ItemSlot.Mount].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon0].ItemId, actualItemsBySlot[ItemSlot.Weapon0].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon1].ItemId, actualItemsBySlot[ItemSlot.Weapon1].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon2].ItemId, actualItemsBySlot[ItemSlot.Weapon2].Item.Id);
            Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon3].ItemId, actualItemsBySlot[ItemSlot.Weapon3].Item.Id);
        }

        [Test]
        public async Task BreakingAllCharacterItemsWithoutAutoRepairShouldBreakThem()
        {
            User user = new()
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                Gold = 10000,
                Characters = new List<Character>
                {
                    new()
                    {
                        Name = "b",
                        EquippedItems =
                        {
                            new EquippedItem { Item = new Item { Rank = 3 }, Slot = ItemSlot.Head },
                            new EquippedItem { Item = new Item { Rank = 2 }, Slot = ItemSlot.Shoulder },
                            new EquippedItem { Item = new Item { Rank = 1 }, Slot = ItemSlot.Body },
                            new EquippedItem { Item = new Item { Rank = 0 }, Slot = ItemSlot.Hand },
                            new EquippedItem { Item = new Item { Rank = -1 }, Slot = ItemSlot.Leg },
                            new EquippedItem { Item = new Item { Rank = -2 }, Slot = ItemSlot.MountHarness },
                            new EquippedItem { Item = new Item { Rank = -3 }, Slot = ItemSlot.Mount },
                            new EquippedItem { Item = new Item { Rank = -2 }, Slot = ItemSlot.Weapon0 },
                            new EquippedItem { Item = new Item { Rank = -1 }, Slot = ItemSlot.Weapon1 },
                            new EquippedItem { Item = new Item { Rank = 0 }, Slot = ItemSlot.Weapon2 },
                            new EquippedItem { Item = new Item { Rank = 1 }, Slot = ItemSlot.Weapon3 },
                        },
                        AutoRepair = false,
                    },
                },
            };
            ArrangeDb.Users.Add(user);

            // for each item, add its base item (rank = 0) and downranked item (rank = rank - 1)
            foreach (var equippedItem in user.Characters[0].EquippedItems)
            {
                ArrangeDb.UserItems.Add(new UserItem { User = user, Item = equippedItem.Item });

                var baseItem = equippedItem.Item!.Rank == 0 ? equippedItem.Item : new Item { Rank = 0 };
                baseItem.BaseItem = baseItem;
                equippedItem.Item.BaseItem = baseItem;
                ArrangeDb.Items.Add(baseItem);

                if (equippedItem.Item.Rank > -3 && equippedItem.Item.Rank - 1 != 0)
                {
                    ArrangeDb.Items.Add(new Item { Rank = equippedItem.Item.Rank - 1, BaseItem = baseItem });
                }
            }

            await ArrangeDb.SaveChangesAsync();

            Mock<ICharacterService> characterServiceMock = new();
            UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object);

            var result = await handler.Handle(new UpdateGameUsersCommand
            {
                Updates = new[]
                {
                    new GameUserUpdate
                    {
                        CharacterId = user.Characters[0].Id,
                        BrokenItems = new[]
                        {
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[0].ItemId, RepairCost = 100 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[1].ItemId, RepairCost = 150 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[2].ItemId, RepairCost = 200 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[3].ItemId, RepairCost = 250 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[4].ItemId, RepairCost = 300 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[5].ItemId, RepairCost = 350 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[6].ItemId, RepairCost = 400 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[7].ItemId, RepairCost = 450 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[8].ItemId, RepairCost = 500 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[9].ItemId, RepairCost = 550 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[10].ItemId, RepairCost = 600 },
                        },
                    },
                },
            }, CancellationToken.None);

            var data = result.Data!;
            Assert.AreEqual(10000, data.UpdateResults[0].User.Gold);
            Assert.AreEqual(11, data.UpdateResults[0].BrokenItems.Count);

            user = await AssertDb.Users
                .Include(u => u.Characters).ThenInclude(c => c.EquippedItems).ThenInclude(ei => ei.Item)
                .FirstAsync(u => u.Id == user.Id);

            var itemsBySlot = user.Characters[0].EquippedItems.ToDictionary(ei => ei.Slot, ei => ei.Item!);
            Assert.AreEqual(2, itemsBySlot[ItemSlot.Head].Rank);
            Assert.AreEqual(1, itemsBySlot[ItemSlot.Shoulder].Rank);
            Assert.AreEqual(0, itemsBySlot[ItemSlot.Body].Rank);
            Assert.AreEqual(-1, itemsBySlot[ItemSlot.Hand].Rank);
            Assert.AreEqual(-2, itemsBySlot[ItemSlot.Leg].Rank);
            Assert.AreEqual(-3, itemsBySlot[ItemSlot.MountHarness].Rank);
            Assert.That(itemsBySlot, Does.Not.ContainKey(ItemSlot.Mount));
            Assert.AreEqual(-3, itemsBySlot[ItemSlot.Weapon0].Rank);
            Assert.AreEqual(-2, itemsBySlot[ItemSlot.Weapon1].Rank);
            Assert.AreEqual(-1, itemsBySlot[ItemSlot.Weapon2].Rank);
            Assert.AreEqual(0, itemsBySlot[ItemSlot.Weapon3].Rank);

            // check broken items were added to user inventory
            foreach (var equippedItem in user.Characters[0].EquippedItems)
            {
                Assert.DoesNotThrowAsync(() => AssertDb.UserItems.FirstAsync(oi => oi.ItemId == equippedItem.ItemId && oi.UserId == user.Id));
            }
        }

        [Test]
        public async Task BreakingCharacterItemsWithAutoRepairShouldRepairUntilThereIsNotEnoughGold()
        {
            Item handItem = new() { Rank = 0 };
            Item downrankedHandItem = new() { Rank = -1, BaseItem = handItem };
            ArrangeDb.Items.AddRange(handItem, downrankedHandItem);

            User user = new()
            {
                Gold = 3000,
                Characters =
                {
                    new Character
                    {
                        EquippedItems =
                        {
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Head },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Shoulder },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Body },
                            new EquippedItem { Item = handItem, Slot = ItemSlot.Hand },
                            new EquippedItem { Item = new Item(), Slot = ItemSlot.Leg },
                        },
                        AutoRepair = true,
                    },
                },
                Items = { new UserItem { Item = handItem } },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            Mock<ICharacterService> characterServiceMock = new();
            UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object);
            var result = await handler.Handle(new UpdateGameUsersCommand
            {
                Updates = new[]
                {
                    new GameUserUpdate
                    {
                        CharacterId = user.Characters[0].Id,
                        BrokenItems = new[]
                        {
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[0].ItemId, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[1].ItemId, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[2].ItemId, RepairCost = 1000 },
                            new GameUserBrokenItem { ItemId = user.Characters[0].EquippedItems[3].ItemId, RepairCost = 1000 },
                        },
                    },
                },
            }, CancellationToken.None);

            var data = result.Data!;
            Assert.AreEqual(0, data.UpdateResults[0].User.Gold);
            Assert.AreEqual(1, data.UpdateResults[0].BrokenItems.Count); // hand
        }
    }
}
