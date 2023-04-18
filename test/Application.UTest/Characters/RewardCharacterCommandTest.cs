using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class RewardCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MinimumRetirementLevel = 2,
    };

    [Test]
    public async Task CharacterNotFound()
    {
        RewardCharacterCommand.Handler handler = new(
            Mock.Of<ICharacterService>(),
            Mock.Of<IExperienceTable>(),
            Mock.Of<IActivityLogService>(),
            ActDb,
            Mapper,
            Constants);
        var res = await handler.Handle(new RewardCharacterCommand
        {
            CharacterId = 1,
            Experience = 1000,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [TestCase(500, 499, 0, 1, 999, Description = "Give experience so they are 1 point from retirement level")]
    [TestCase(500, 500, 1, 1, 0, Description = "Give experience so they are exactly at retirement level")]
    [TestCase(500, 1500, 2, 1, 0, Description = "Give enough experience to retire twice")]
    [TestCase(500, 2000, 2, 1, 500, Description = "Give enough experience to retire twice and still have some experience left")]
    [TestCase(1200, 2000, 3, 1, 0, Description = "Give enough experience to retire twice but but they are above retirement level")]
    [TestCase(1200, 2500, 3, 1, 500, Description = "Give enough experience to retire twice but but they are above retirement level and still experience left a the end")]
    public async Task AutoRetire(int startExperience, int experienceReward, int expectedGeneration, int expectedLevel, int expectedExperience)
    {
        const int experienceForRetirementLevel = 1000;
        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock
            .Setup(t => t.GetExperienceForLevel(Constants.MinimumRetirementLevel))
            .Returns(experienceForRetirementLevel);
        experienceTableMock
            .Setup(t => t.GetLevelForExperience(It.IsAny<int>()))
            .Returns((Func<int, int>)(xp => xp switch
            {
                experienceForRetirementLevel => Constants.MinimumRetirementLevel,
                < experienceForRetirementLevel => Constants.MinimumRetirementLevel - 1,
                > experienceForRetirementLevel => Constants.MinimumRetirementLevel + 1,
            }));

        Character character = new()
        {
            Generation = 0,
            Level = experienceTableMock.Object.GetLevelForExperience(startExperience),
            Experience = startExperience,
            User = new User
            {
                HeirloomPoints = 1,
                ExperienceMultiplier = 2f, // Should be ignored.
            },
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        CharacterService characterService = new(experienceTableMock.Object, Constants);

        RewardCharacterCommand.Handler handler = new(
            characterService,
            experienceTableMock.Object,
            activityLogServiceMock.Object,
            ActDb,
            Mapper,
            Constants);
        var res = await handler.Handle(new RewardCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.User.Id,
            Experience = experienceReward,
            AutoRetire = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Generation, Is.EqualTo(expectedGeneration));
        Assert.That(res.Data.Level, Is.EqualTo(expectedLevel));
        Assert.That(res.Data.Experience, Is.EqualTo(expectedExperience));
    }

    [Test]
    public async Task NoAutoRetire()
    {
        Character character = new()
        {
            Generation = 0,
            Level = 10,
            Experience = 100,
            User = new User
            {
                HeirloomPoints = 1,
                ExperienceMultiplier = 2f, // Should be ignored.
            },
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        Mock<IExperienceTable> experienceTableMock = new();

        RewardCharacterCommand.Handler handler = new(
            characterServiceMock.Object,
            experienceTableMock.Object,
            activityLogServiceMock.Object,
            ActDb,
            Mapper,
            Constants);
        var res = await handler.Handle(new RewardCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.User.Id,
            Experience = 100,
            AutoRetire = false,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);

        characterServiceMock.Verify(s => s.GiveExperience(It.IsAny<Character>(), 100, false));
    }
}
