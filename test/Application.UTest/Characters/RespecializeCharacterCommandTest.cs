using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class RespecializeCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        RespecializeExperiencePenaltyCoefs = new[] { 0.5f, 0f },
    };

    [Test]
    public async Task RespecializeCharacterLevel3ShouldMakeItLevel2()
    {
        Character character = new()
        {
            Generation = 2,
            Level = 3,
            Experience = 150,
            ExperienceMultiplier = 1.1f,
            SkippedTheFun = false,
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
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);

        Mock<ICharacterService> characterServiceMock = new();

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, experienceTableMock.Object, Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.AreEqual(2, character.Generation);
        Assert.AreEqual(2, character.Level);
        Assert.AreEqual(75, character.Experience);
        Assert.AreEqual(1.1f, character.ExperienceMultiplier);
        Assert.IsEmpty(character.EquippedItems);
        Assert.AreEqual(0, character.Statistics.Kills);
        Assert.AreEqual(0, character.Statistics.Deaths);
        Assert.AreEqual(0, character.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(4), character.Statistics.PlayTime);
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task RespecializeSkippedTheFunCharacterShouldNotChangeLevel()
    {
        Character character = new()
        {
            Generation = 0,
            Level = 3,
            Experience = 150,
            ExperienceMultiplier = 1.1f,
            SkippedTheFun = true,
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
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, experienceTableMock.Object, Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.AreEqual(0, character.Generation);
        Assert.AreEqual(3, character.Level);
        Assert.AreEqual(150, character.Experience);
        Assert.AreEqual(1.1f, character.ExperienceMultiplier);
        Assert.IsEmpty(character.EquippedItems);
        Assert.AreEqual(0, character.Statistics.Kills);
        Assert.AreEqual(0, character.Statistics.Deaths);
        Assert.AreEqual(0, character.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(4), character.Statistics.PlayTime);
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserDoesntExist()
    {
        var experienceTable = Mock.Of<IExperienceTable>();
        var characterService = Mock.Of<ICharacterService>();
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable, Constants);
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
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable, Constants);
        var result = await handler.Handle(
            new RespecializeCharacterCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }
}
