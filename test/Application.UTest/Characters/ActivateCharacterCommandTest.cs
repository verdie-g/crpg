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

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
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

        Assert.IsNotNull(result.Errors);
        Assert.AreEqual(ErrorCode.CharacterForTournament, result.Errors![0].Code);
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

        Assert.IsNull(result.Errors);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.AreEqual(user.Characters[0].Id, userDb.ActiveCharacterId);
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

        Assert.IsNull(result.Errors);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.IsNull(userDb.ActiveCharacter);
        Assert.IsNull(userDb.ActiveCharacterId);
    }
}
