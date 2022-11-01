using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;

public class UpdateGameUsersCommandTest : TestBase
{
    [Test]
    public void ShouldDoNothingForEmptyUpdates()
    {
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IItemService> itemServiceMock = new();
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, itemServiceMock.Object);
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
                    EquippedItems =
                    {
                        new EquippedItem
                        {
                            UserItem = new UserItem { BaseItem = new Item() },
                            Slot = ItemSlot.Body,
                        },
                    },
                    Statistics = new CharacterStatistics
                    {
                        Kills = 1,
                        Deaths = 2,
                        Assists = 3,
                        PlayTime = TimeSpan.FromSeconds(4),
                    },
                    Rating = new CharacterRating
                    {
                        Value = 1,
                        Deviation = 2,
                        Volatility = 3,
                    },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        characterServiceMock
            .Setup(cs => cs.GiveExperience(It.IsAny<Character>(), 10))
            .Callback((Character c, int xp) => c.Experience += xp);
        Mock<IItemService> itemServiceMock = new();
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, itemServiceMock.Object);
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
                    Statistics = new CharacterStatisticsViewModel
                    {
                        Kills = 5,
                        Deaths = 6,
                        Assists = 7,
                        PlayTime = TimeSpan.FromSeconds(8),
                    },
                    // Rating = new CharacterRatingViewModel
                    // {
                    //     Value = 4,
                    //     Deviation = 5,
                    //     Volatility = 6,
                    // },
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
        Assert.IsEmpty(data.UpdateResults[0].User.Restrictions);
        Assert.AreEqual(10, data.UpdateResults[0].EffectiveReward.Experience);
        Assert.AreEqual(200, data.UpdateResults[0].EffectiveReward.Gold);
        Assert.IsFalse(data.UpdateResults[0].EffectiveReward.LevelUp);
        Assert.IsEmpty(data.UpdateResults[0].RepairedItems);

        var dbCharacter = await AssertDb.Characters.FirstAsync(c => c.Id == user.Characters[0].Id);
        Assert.AreEqual(6, dbCharacter.Statistics.Kills);
        Assert.AreEqual(8, dbCharacter.Statistics.Deaths);
        Assert.AreEqual(10, dbCharacter.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(12), dbCharacter.Statistics.PlayTime);
        // Assert.AreEqual(4, dbCharacter.Rating.Value);
        // Assert.AreEqual(5, dbCharacter.Rating.Deviation);
        // Assert.AreEqual(6, dbCharacter.Rating.Volatility);

        characterServiceMock.VerifyAll();
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
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "0" } }, Slot = ItemSlot.Head },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "1" } }, Slot = ItemSlot.Shoulder },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "2" } }, Slot = ItemSlot.Body },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "3" } }, Slot = ItemSlot.Hand },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "4" } }, Slot = ItemSlot.Leg },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "5" } }, Slot = ItemSlot.MountHarness },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "6" } }, Slot = ItemSlot.Mount },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "7" } }, Slot = ItemSlot.Weapon0 },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "8" } }, Slot = ItemSlot.Weapon1 },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "9" } }, Slot = ItemSlot.Weapon2 },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "10" } }, Slot = ItemSlot.Weapon3 },
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item { Id = "11" } }, Slot = ItemSlot.WeaponExtra },
                    },
                    AutoRepair = true,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        Mock<IItemService> itemServiceMock = new();
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, itemServiceMock.Object);
        var result = await handler.Handle(new UpdateGameUsersCommand
        {
            Updates = new[]
            {
                new GameUserUpdate
                {
                    CharacterId = user.Characters[0].Id,
                    BrokenItems = new[]
                    {
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[0].UserItemId, RepairCost = 100 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[1].UserItemId, RepairCost = 150 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[2].UserItemId, RepairCost = 200 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[3].UserItemId, RepairCost = 250 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[4].UserItemId, RepairCost = 300 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[5].UserItemId, RepairCost = 350 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[6].UserItemId, RepairCost = 400 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[7].UserItemId, RepairCost = 450 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[8].UserItemId, RepairCost = 500 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[9].UserItemId, RepairCost = 550 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[10].UserItemId, RepairCost = 600 },
                        new GameUserBrokenItem { UserItemId = user.Characters[0].EquippedItems[11].UserItemId, RepairCost = 650 },
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.AreEqual(10000 - 4500, data.UpdateResults[0].User.Gold);
        Assert.AreEqual(12, data.UpdateResults[0].RepairedItems.Count);

        var expectedItemsBySlot = user.Characters[0].EquippedItems.ToDictionary(ei => ei.Slot);
        var actualItemsBySlot = data.UpdateResults[0].User.Character.EquippedItems.ToDictionary(ei => ei.Slot);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Head].UserItemId, actualItemsBySlot[ItemSlot.Head].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Shoulder].UserItemId, actualItemsBySlot[ItemSlot.Shoulder].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Body].UserItemId, actualItemsBySlot[ItemSlot.Body].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Hand].UserItemId, actualItemsBySlot[ItemSlot.Hand].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Leg].UserItemId, actualItemsBySlot[ItemSlot.Leg].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.MountHarness].UserItemId, actualItemsBySlot[ItemSlot.MountHarness].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Mount].UserItemId, actualItemsBySlot[ItemSlot.Mount].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon0].UserItemId, actualItemsBySlot[ItemSlot.Weapon0].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon1].UserItemId, actualItemsBySlot[ItemSlot.Weapon1].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon2].UserItemId, actualItemsBySlot[ItemSlot.Weapon2].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.Weapon3].UserItemId, actualItemsBySlot[ItemSlot.Weapon3].UserItem.Id);
        Assert.AreEqual(expectedItemsBySlot[ItemSlot.WeaponExtra].UserItemId, actualItemsBySlot[ItemSlot.WeaponExtra].UserItem.Id);
    }

    [Test]
    [Ignore("Downgrading items is not ready")]
    public async Task BreakingAllCharacterItemsWithoutAutoRepairShouldBreakThem()
    {
        UserItem userItem0 = new() { Rank = 3, BaseItem = new Item { Id = "0" } };
        UserItem userItem1 = new() { Rank = 2, BaseItem = new Item { Id = "1" } };
        UserItem userItem2 = new() { Rank = 1, BaseItem = new Item { Id = "2" } };
        UserItem userItem3 = new() { Rank = 0, BaseItem = new Item { Id = "3" } };
        UserItem userItem4 = new() { Rank = -1, BaseItem = new Item { Id = "4" } };
        UserItem userItem5 = new() { Rank = -2, BaseItem = new Item { Id = "5" } };
        UserItem userItem6 = new() { Rank = -3, BaseItem = new Item { Id = "6" } };
        UserItem userItem7 = new() { Rank = -2, BaseItem = new Item { Id = "7" } };
        UserItem userItem8 = new() { Rank = -1, BaseItem = new Item { Id = "8" } };
        UserItem userItem9 = new() { Rank = 0, BaseItem = new Item { Id = "9" } };
        UserItem userItem10 = new() { Rank = 1, BaseItem = new Item { Id = "10" } };

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
                        new EquippedItem { UserItem = userItem0, Slot = ItemSlot.Head },
                        new EquippedItem { UserItem = userItem1, Slot = ItemSlot.Shoulder },
                        new EquippedItem { UserItem = userItem2, Slot = ItemSlot.Body },
                        new EquippedItem { UserItem = userItem3, Slot = ItemSlot.Hand },
                        new EquippedItem { UserItem = userItem4, Slot = ItemSlot.Leg },
                        new EquippedItem { UserItem = userItem5, Slot = ItemSlot.MountHarness },
                        new EquippedItem { UserItem = userItem6, Slot = ItemSlot.Mount },
                        new EquippedItem { UserItem = userItem7, Slot = ItemSlot.Weapon0 },
                        new EquippedItem { UserItem = userItem8, Slot = ItemSlot.Weapon1 },
                        new EquippedItem { UserItem = userItem9, Slot = ItemSlot.Weapon2 },
                        new EquippedItem { UserItem = userItem10, Slot = ItemSlot.Weapon3 },
                    },
                    AutoRepair = false,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        Mock<IItemService> itemServiceMock = new();
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, itemServiceMock.Object);

        var result = await handler.Handle(new UpdateGameUsersCommand
        {
            Updates = new[]
            {
                new GameUserUpdate
                {
                    CharacterId = user.Characters[0].Id,
                    BrokenItems = new[]
                    {
                        new GameUserBrokenItem { UserItemId = userItem0.Id, RepairCost = 100 },
                        new GameUserBrokenItem { UserItemId = userItem1.Id, RepairCost = 150 },
                        new GameUserBrokenItem { UserItemId = userItem2.Id, RepairCost = 200 },
                        new GameUserBrokenItem { UserItemId = userItem3.Id, RepairCost = 250 },
                        new GameUserBrokenItem { UserItemId = userItem4.Id, RepairCost = 300 },
                        new GameUserBrokenItem { UserItemId = userItem5.Id, RepairCost = 350 },
                        new GameUserBrokenItem { UserItemId = userItem6.Id, RepairCost = 400 },
                        new GameUserBrokenItem { UserItemId = userItem7.Id, RepairCost = 450 },
                        new GameUserBrokenItem { UserItemId = userItem8.Id, RepairCost = 500 },
                        new GameUserBrokenItem { UserItemId = userItem9.Id, RepairCost = 550 },
                        new GameUserBrokenItem { UserItemId = userItem10.Id, RepairCost = 600 },
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.AreEqual(10000, data.UpdateResults[0].User.Gold);
        Assert.AreEqual(11, data.UpdateResults[0].RepairedItems.Count);

        user = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems).ThenInclude(ei => ei.UserItem)
            .FirstAsync(u => u.Id == user.Id);

        var userItemsBySlot = user.Characters[0].EquippedItems.ToDictionary(ei => ei.Slot, ei => ei.UserItem!);
        Assert.AreEqual(2, userItemsBySlot[ItemSlot.Head].Rank);
        Assert.AreEqual(1, userItemsBySlot[ItemSlot.Shoulder].Rank);
        Assert.AreEqual(0, userItemsBySlot[ItemSlot.Body].Rank);
        Assert.AreEqual(-1, userItemsBySlot[ItemSlot.Hand].Rank);
        Assert.AreEqual(-2, userItemsBySlot[ItemSlot.Leg].Rank);
        Assert.AreEqual(-3, userItemsBySlot[ItemSlot.MountHarness].Rank);
        Assert.AreEqual(-3, userItemsBySlot[ItemSlot.Mount].Rank);
        Assert.AreEqual(-3, userItemsBySlot[ItemSlot.Weapon0].Rank);
        Assert.AreEqual(-2, userItemsBySlot[ItemSlot.Weapon1].Rank);
        Assert.AreEqual(-1, userItemsBySlot[ItemSlot.Weapon2].Rank);
        Assert.AreEqual(0, userItemsBySlot[ItemSlot.Weapon3].Rank);
    }

    [Test]
    public async Task BreakingCharacterItemsWithAutoRepairShouldRepairUntilThereIsNotEnoughGold()
    {
        UserItem userItem0 = new() { Rank = 0, BaseItem = new Item { Id = "0" } };
        UserItem userItem1 = new() { Rank = 0, BaseItem = new Item { Id = "1" } };
        UserItem userItem2 = new() { Rank = 0, BaseItem = new Item { Id = "2" } };
        UserItem userItem3 = new() { Rank = 0, BaseItem = new Item { Id = "3" } };
        UserItem userItem4 = new() { Rank = 0, BaseItem = new Item { Id = "4" } };

        User user = new()
        {
            Gold = 3000,
            Characters =
            {
                new Character
                {
                    EquippedItems =
                    {
                        new EquippedItem { UserItem = userItem0, Slot = ItemSlot.Head },
                        new EquippedItem { UserItem = userItem1, Slot = ItemSlot.Shoulder },
                        new EquippedItem { UserItem = userItem2, Slot = ItemSlot.Body },
                        new EquippedItem { UserItem = userItem3, Slot = ItemSlot.Hand },
                        new EquippedItem { UserItem = userItem4, Slot = ItemSlot.Leg },
                    },
                    AutoRepair = true,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        Mock<IItemService> itemServiceMock = new();
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, itemServiceMock.Object);
        var result = await handler.Handle(new UpdateGameUsersCommand
        {
            Updates = new[]
            {
                new GameUserUpdate
                {
                    CharacterId = user.Characters[0].Id,
                    BrokenItems = new[]
                    {
                        new GameUserBrokenItem { UserItemId = userItem0.Id, RepairCost = 1000 },
                        new GameUserBrokenItem { UserItemId = userItem1.Id, RepairCost = 1000 },
                        new GameUserBrokenItem { UserItemId = userItem2.Id, RepairCost = 1000 },
                        new GameUserBrokenItem { UserItemId = userItem3.Id, RepairCost = 1000 },
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.AreEqual(0, data.UpdateResults[0].User.Gold);
        Assert.AreEqual(4, data.UpdateResults[0].RepairedItems.Count);
        Assert.AreEqual(1, data.UpdateResults[0].RepairedItems.Count(i => i.Sold)); // not enough gold to repair the last one.
    }
}
