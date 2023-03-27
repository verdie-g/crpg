using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class SetCharacterForTournamentCommandTest : TestBase
{
    private static readonly Constants Constants = new() { TournamentLevel = 30 };

    [Theory]
    public async Task ShouldSetTournamentForCharacterCorrectly(bool activeCharacter)
    {
        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(30000);

        Mock<ICharacterService> characterServiceMock = new();

        User user = new();
        Character character = new() { Generation = 0, Level = 3, Experience = 250 };
        user.Characters.Add(character);
        user.ActiveCharacter = activeCharacter ? character : new Character();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SetCharacterForTournamentCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, experienceTableMock.Object, Constants);
        var result = await handler.Handle(new SetCharacterForTournamentCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        character = AssertDb.Characters.First(c => c.Id == character.Id);
        Assert.That(character.Generation, Is.EqualTo(0));
        Assert.That(character.Level, Is.EqualTo(30));
        Assert.That(character.Experience, Is.EqualTo(30000));
        Assert.That(character.ForTournament, Is.True);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        if (activeCharacter)
        {
            Assert.That(userDb.ActiveCharacterId, Is.Null);
        }
        else
        {
            Assert.That(userDb.ActiveCharacterId, Is.Not.Null);
        }

        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true), Times.Once);
    }

    [Test]
    public async Task ShouldReturnErrorIfCharacterIsGeneration1()
    {
        Character character = new() { Generation = 1, Level = 3, Experience = 250, User = new() };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        SetCharacterForTournamentCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, experienceTableMock.Object, Constants);
        var result = await handler.Handle(new SetCharacterForTournamentCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterGenerationRequirement));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfCharacterNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        SetCharacterForTournamentCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterService>(),
            Mock.Of<IExperienceTable>(), Constants);
        var result = await handler.Handle(new SetCharacterForTournamentCommand
        {
            UserId = user.Id,
            CharacterId = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
