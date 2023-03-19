using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class RespecializeCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        RespecializeExperiencePenaltyCoefs = new[] { 0.5f, 0f },
        RespecializePriceForLevel30 = 5000,
        FreeRespecializeIntervalDays = 7,
    };

    [Theory]
    public async Task RespecializeCharacterLevel3ShouldMakeItLevel2(bool freeRespec)
    {
        Character character = new()
        {
            Generation = 2,
            Level = 3,
            Experience = 150,
            ForTournament = false,
            EquippedItems =
            {
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Head },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Body },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Weapon0 },
            },
            Statistics = new CharacterStatistics
            {
                Kills = 1,
                Deaths = 2,
                Assists = 3,
                PlayTime = TimeSpan.FromSeconds(4),
            },
            User = new() { Gold = 1000 },
            Limitations = new CharacterLimitations
            {
                LastFreeRespecializeAt = freeRespec
                    ? new DateTime(2023, 3, 9)
                    : new DateTime(2023, 3, 16),
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);
        experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(100000);

        Mock<ICharacterService> characterServiceMock = new();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2023, 3, 17));

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
            experienceTableMock.Object, activityLogServiceMock.Object, dateTimeMock.Object, Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        var characterDb = await AssertDb.Characters
            .Include(c => c.Limitations)
            .FirstAsync(c => c.Id == character.Id);
        if (freeRespec)
        {
            Assert.AreEqual(character.User!.Gold, 1000);
            Assert.AreEqual(dateTimeMock.Object.UtcNow, characterDb.Limitations!.LastFreeRespecializeAt);
        }
        else
        {
            Assert.Less(character.User!.Gold, 1000);
            Assert.AreEqual(new DateTime(2023, 3, 16), characterDb.Limitations!.LastFreeRespecializeAt);
        }

        Assert.AreEqual(2, character.Generation);
        Assert.AreEqual(2, character.Level);
        Assert.AreEqual(75, character.Experience);
        Assert.AreEqual(3, character.EquippedItems.Count);
        Assert.AreEqual(1, character.Statistics.Kills);
        Assert.AreEqual(2, character.Statistics.Deaths);
        Assert.AreEqual(3, character.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(4), character.Statistics.PlayTime);
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task RespecializeTournamentCharacterShouldNotChangeLevelOrGold()
    {
        Character character = new()
        {
            Generation = 0,
            Level = 3,
            Experience = 150,
            ForTournament = true,
            EquippedItems =
            {
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Head },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Body },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Weapon0 },
            },
            Statistics = new CharacterStatistics
            {
                Kills = 1,
                Deaths = 2,
                Assists = 3,
                PlayTime = TimeSpan.FromSeconds(4),
            },
            User = new() { Gold = 500 },
            Limitations = new CharacterLimitations { LastFreeRespecializeAt = DateTime.UtcNow - TimeSpan.FromDays(1) },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
            experienceTableMock.Object, activityLogServiceMock.Object, new MachineDateTime(), Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.AreEqual(500, character.User!.Gold);
        Assert.AreEqual(0, character.Generation);
        Assert.AreEqual(3, character.Level);
        Assert.AreEqual(150, character.Experience);
        Assert.AreEqual(3, character.EquippedItems.Count);
        Assert.AreEqual(1, character.Statistics.Kills);
        Assert.AreEqual(2, character.Statistics.Deaths);
        Assert.AreEqual(3, character.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(4), character.Statistics.PlayTime);
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task ShouldReturnErrorIfNoEnoughGold()
    {
        Character character = new()
        {
            Generation = 2,
            Level = 3,
            Experience = 150,
            ForTournament = false,
            User = new() { Gold = 0 },
            Limitations = new CharacterLimitations { LastFreeRespecializeAt = DateTime.UtcNow },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);
        experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(100000);

        Mock<ICharacterService> characterServiceMock = new();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
            experienceTableMock.Object, activityLogServiceMock.Object, new MachineDateTime(), Constants);
        var res = await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.NotEnoughGold, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserDoesntExist()
    {
        var experienceTable = Mock.Of<IExperienceTable>();
        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable,
            activityLogService, new MachineDateTime(), Constants);
        var result = await handler.Handle(
            new RespecializeCharacterCommand
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfCharacterDoesntExist()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var experienceTable = Mock.Of<IExperienceTable>();
        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable,
            activityLogService, new MachineDateTime(), Constants);
        var result = await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = 1,
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }
}
