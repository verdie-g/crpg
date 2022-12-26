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

public class RetireCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MinimumRetirementLevel = 31,
        ExperienceMultiplierByGeneration = 0.03f,
        MaxExperienceMultiplierForGeneration = 1.48f,
    };

    [TestCase(31, 1.00f, 1, 1.03f)]
    [TestCase(32, 1.00f, 1, 1.03f)]
    [TestCase(33, 1.00f, 2, 1.03f)]
    [TestCase(34, 1.00f, 2, 1.03f)]
    [TestCase(35, 1.00f, 3, 1.03f)]
    [TestCase(36, 1.00f, 3, 1.03f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.45f, 1, 1.48f)]
    [TestCase(31, 1.48f, 1, 1.48f)]
    public async Task Basic(int level, float experienceMultiplier, int expectedPoints, float expectedExperienceMultiplier)
    {
        Character character = new()
        {
            Generation = 0,
            Level = level,
            Experience = 32000,
            EquippedItems =
            {
                new EquippedItem { Slot = ItemSlot.Head },
                new EquippedItem { Slot = ItemSlot.Hand },
            },
            Statistics = new CharacterStatistics
            {
                Kills = 1,
                Deaths = 2,
                Assists = 3,
                PlayTime = TimeSpan.FromSeconds(4),
            },
            User = new User
            {
                HeirloomPoints = 0,
                ExperienceMultiplier = experienceMultiplier,
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();

        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, Constants);
        await handler.Handle(new RetireCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.AreEqual(1, character.Generation);
        Assert.AreEqual(Constants.MinimumLevel, character.Level);
        Assert.AreEqual(0, character.Experience);
        Assert.AreEqual(0, character.Statistics.Kills);
        Assert.AreEqual(0, character.Statistics.Deaths);
        Assert.AreEqual(0, character.Statistics.Assists);
        Assert.AreEqual(TimeSpan.FromSeconds(4), character.Statistics.PlayTime);
        Assert.AreEqual(expectedPoints, character.User!.HeirloomPoints);
        Assert.AreEqual(expectedExperienceMultiplier, character.User.ExperienceMultiplier, delta: 0.001f);
        Assert.IsEmpty(character.EquippedItems);

        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), false));
    }

    [Test]
    public async Task NotFoundIfUserDoesntExist()
    {
        var characterService = Mock.Of<ICharacterService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task NotFoundIfCharacterDoesntExist()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var characterService = Mock.Of<ICharacterService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task BadRequestIfLevelTooLow()
    {
        Character character = new()
        {
            Level = 30,
            User = new User(),
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        var characterService = Mock.Of<ICharacterService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterLevelRequirementNotMet, result.Errors![0].Code);
    }
}
