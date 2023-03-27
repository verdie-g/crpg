using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class ActivateCharacterCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterNotFound()
    {
        User user = new()
        {
            Characters = new List<Character>(),
            ActiveCharacter = null,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new ActivateCharacterCommand.Handler(ActDb).Handle(new ActivateCharacterCommand
        {
            CharacterId = 1,
            UserId = user.Id,
            Active = true,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfCharacterIsForTournament()
    {
        User user = new()
        {
            Characters = { new Character { ForTournament = true } },
            ActiveCharacter = null,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new ActivateCharacterCommand.Handler(ActDb).Handle(new ActivateCharacterCommand
        {
            CharacterId = user.Characters[0].Id,
            UserId = user.Id,
            Active = true,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterForTournament));
    }

    [Test]
    public async Task ShouldActivateCharacterWithTrue()
    {
        User user = new()
        {
            Characters = { new Character() },
            ActiveCharacter = null,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new ActivateCharacterCommand.Handler(ActDb).Handle(new ActivateCharacterCommand
        {
            CharacterId = user.Characters[0].Id,
            UserId = user.Id,
            Active = true,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.ActiveCharacterId, Is.EqualTo(user.Characters[0].Id));
    }

    [Test]
    public async Task ShouldDeactivateCharacterWithFalse()
    {
        User user = new()
        {
            Characters = { new Character() },
            ActiveCharacter = new Character(),
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new ActivateCharacterCommand.Handler(ActDb).Handle(new ActivateCharacterCommand
        {
            CharacterId = user.Characters[0].Id,
            UserId = user.Id,
            Active = false,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.ActiveCharacter, Is.Null);
        Assert.That(userDb.ActiveCharacterId, Is.Null);
    }
}
