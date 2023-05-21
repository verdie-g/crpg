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
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        Mock<CompetitiveRatingModel> competitiveRatingModelMock = new() { DefaultValue = DefaultValue.Mock };
        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, activityLogServiceMock.Object, competitiveRatingModelMock.Object);
        Assert.That(() => handler.Handle(new UpdateGameUsersCommand(), CancellationToken.None), Throws.Nothing);
    }

    [Test]
    public async Task ShouldUpdateExistingCharacterCorrectly()
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Gold = 1000,
            ExperienceMultiplier = 1.0f,
            Characters = new List<Character>
            {
                new()
                {
                    Name = "a",
                    Experience = 0,
                    Level = 1,
                    EquippedItems =
                    {
                        new EquippedItem
                        {
                            UserItem = new UserItem { Item = new Item() },
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
            .Setup(cs => cs.GiveExperience(It.IsAny<Character>(), 10, true))
            .Callback((Character c, int xp, bool _) => c.Experience += xp);

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        Mock<CompetitiveRatingModel> competitiveRatingModelMock = new() { DefaultValue = DefaultValue.Mock };

        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, activityLogServiceMock.Object, competitiveRatingModelMock.Object);
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
                    Rating = new CharacterRatingViewModel
                    {
                        Value = 4,
                        Deviation = 5,
                        Volatility = 6,
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.That(data.UpdateResults.Count, Is.EqualTo(1));
        Assert.That(data.UpdateResults[0].User.Id, Is.EqualTo(user.Id));
        Assert.That(data.UpdateResults[0].User.Platform, Is.EqualTo(Platform.Steam));
        Assert.That(data.UpdateResults[0].User.PlatformUserId, Is.EqualTo("1"));
        Assert.That(data.UpdateResults[0].User.Gold, Is.EqualTo(1000 + 200));
        Assert.That(data.UpdateResults[0].User.Character.Name, Is.EqualTo("a"));
        Assert.That(data.UpdateResults[0].User.Character.EquippedItems.Count, Is.EqualTo(1));
        Assert.That(data.UpdateResults[0].User.Restrictions, Is.Empty);
        Assert.That(data.UpdateResults[0].EffectiveReward.Experience, Is.EqualTo(10));
        Assert.That(data.UpdateResults[0].EffectiveReward.Gold, Is.EqualTo(200));
        Assert.That(data.UpdateResults[0].EffectiveReward.LevelUp, Is.False);
        Assert.That(data.UpdateResults[0].RepairedItems, Is.Empty);

        var dbCharacter = await AssertDb.Characters.FirstAsync(c => c.Id == user.Characters[0].Id);
        Assert.That(dbCharacter.Statistics.Kills, Is.EqualTo(6));
        Assert.That(dbCharacter.Statistics.Deaths, Is.EqualTo(8));
        Assert.That(dbCharacter.Statistics.Assists, Is.EqualTo(10));
        Assert.That(dbCharacter.Statistics.PlayTime, Is.EqualTo(TimeSpan.FromSeconds(12)));
        Assert.That(dbCharacter.Rating.Value, Is.EqualTo(4));
        Assert.That(dbCharacter.Rating.Deviation, Is.EqualTo(5));
        Assert.That(dbCharacter.Rating.Volatility, Is.EqualTo(6));

        characterServiceMock.VerifyAll();
    }

    [Test]
    public async Task BreakingAllCharacterItemsShouldRepairThemIfEnoughGold()
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
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "0" } }, Slot = ItemSlot.Head },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "1" } }, Slot = ItemSlot.Shoulder },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "2" } }, Slot = ItemSlot.Body },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "3" } }, Slot = ItemSlot.Hand },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "4" } }, Slot = ItemSlot.Leg },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "5" } }, Slot = ItemSlot.MountHarness },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "6" } }, Slot = ItemSlot.Mount },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "7" } }, Slot = ItemSlot.Weapon0 },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "8" } }, Slot = ItemSlot.Weapon1 },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "9" } }, Slot = ItemSlot.Weapon2 },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "10" } }, Slot = ItemSlot.Weapon3 },
                        new EquippedItem { UserItem = new UserItem { Item = new Item { Id = "11" } }, Slot = ItemSlot.WeaponExtra },
                    },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        Mock<CompetitiveRatingModel> competitiveRatingModelMock = new() { DefaultValue = DefaultValue.Mock };

        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, activityLogServiceMock.Object, competitiveRatingModelMock.Object);
        var result = await handler.Handle(new UpdateGameUsersCommand
        {
            Updates = new[]
            {
                new GameUserUpdate
                {
                    CharacterId = user.Characters[0].Id,
                    BrokenItems = new[]
                    {
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[0].UserItemId, RepairCost = 100 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[1].UserItemId, RepairCost = 150 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[2].UserItemId, RepairCost = 200 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[3].UserItemId, RepairCost = 250 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[4].UserItemId, RepairCost = 300 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[5].UserItemId, RepairCost = 350 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[6].UserItemId, RepairCost = 400 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[7].UserItemId, RepairCost = 450 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[8].UserItemId, RepairCost = 500 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[9].UserItemId, RepairCost = 550 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[10].UserItemId, RepairCost = 600 },
                        new GameUserDamagedItem { UserItemId = user.Characters[0].EquippedItems[11].UserItemId, RepairCost = 650 },
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.That(data.UpdateResults[0].User.Gold, Is.EqualTo(10000 - 4500));
        Assert.That(data.UpdateResults[0].RepairedItems.Count, Is.EqualTo(12));

        var expectedItemsBySlot = user.Characters[0].EquippedItems.ToDictionary(ei => ei.Slot);
        var actualItemsBySlot = data.UpdateResults[0].User.Character.EquippedItems.ToDictionary(ei => ei.Slot);
        Assert.That(actualItemsBySlot[ItemSlot.Head].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Head].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Shoulder].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Shoulder].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Body].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Body].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Hand].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Hand].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Leg].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Leg].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.MountHarness].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.MountHarness].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Mount].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Mount].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Weapon0].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Weapon0].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Weapon1].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Weapon1].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Weapon2].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Weapon2].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.Weapon3].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.Weapon3].UserItemId));
        Assert.That(actualItemsBySlot[ItemSlot.WeaponExtra].UserItem.Id, Is.EqualTo(expectedItemsBySlot[ItemSlot.WeaponExtra].UserItemId));
    }

    [Test]
    public async Task BreakingCharacterItemsShouldRepairUntilThereIsNotEnoughGold()
    {
        UserItem userItem0 = new() { Item = new Item { Id = "0", Rank = 0 } };
        UserItem userItem1 = new() { Item = new Item { Id = "1", Rank = 0 } };
        UserItem userItem2 = new() { Item = new Item { Id = "2", Rank = 0 } };
        UserItem userItem3 = new() { Item = new Item { Id = "3", Rank = 0 } };
        UserItem userItem4 = new() { Item = new Item { Id = "4", Rank = 0 } };

        User user = new()
        {
            Gold = 2000,
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
                },
                new Character
                {
                    EquippedItems =
                    {
                        new EquippedItem { UserItem = userItem0, Slot = ItemSlot.Head },
                        new EquippedItem { UserItem = userItem2, Slot = ItemSlot.Body },
                        new EquippedItem { UserItem = userItem3, Slot = ItemSlot.Hand },
                    },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        Mock<CompetitiveRatingModel> competitiveRatingModelMock = new() { DefaultValue = DefaultValue.Mock };

        UpdateGameUsersCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, activityLogServiceMock.Object, competitiveRatingModelMock.Object);
        var result = await handler.Handle(new UpdateGameUsersCommand
        {
            Updates = new[]
            {
                new GameUserUpdate
                {
                    CharacterId = user.Characters[0].Id,
                    BrokenItems = new[]
                    {
                        new GameUserDamagedItem { UserItemId = userItem0.Id, RepairCost = 1000 },
                        new GameUserDamagedItem { UserItemId = userItem1.Id, RepairCost = 1000 },
                        new GameUserDamagedItem { UserItemId = userItem2.Id, RepairCost = 1000 },
                        new GameUserDamagedItem { UserItemId = userItem3.Id, RepairCost = 1000 },
                    },
                },
            },
        }, CancellationToken.None);

        var data = result.Data!;
        Assert.That(data.UpdateResults[0].User.Gold, Is.EqualTo(0));
        Assert.That(data.UpdateResults[0].User.Character.EquippedItems.Select(ei => ei.UserItem.Id),
            Is.EquivalentTo(new[] { userItem0.Id, userItem1.Id, userItem4.Id }));

        Assert.That(data.UpdateResults[0].RepairedItems.Count, Is.EqualTo(4));
        Assert.That(data.UpdateResults[0].RepairedItems.Count(i => i.Broke), Is.EqualTo(2));
        Assert.That(data.UpdateResults[0].RepairedItems.Sum(i => i.RepairCost), Is.EqualTo(2000));

        // Check the user's second character got his item equipped too.
        var characterDb1 = await AssertDb.Characters
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == user.Characters[1].Id);
        Assert.That(characterDb1.EquippedItems.Select(ei => ei.UserItemId),
            Is.EquivalentTo(new[] { userItem0.Id }));

        // Check the user item ranks were set to -1.
        var userItemsDb = await AssertDb.UserItems
            .Where(ui => new[] { userItem2.Id, userItem3.Id }.Contains(ui.Id))
            .ToArrayAsync();
        foreach (var userItem in userItemsDb)
        {
            Assert.That(userItem.IsBroken, Is.True);
        }
    }
}
