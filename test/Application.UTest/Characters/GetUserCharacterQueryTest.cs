using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserCharacterQueryTest : TestBase
{
    [Test]
    public async Task WhenCharacterDoesntExist()
    {
        GetUserCharacterQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterQuery
        {
            CharacterId = 1,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task WhenCharacterExists()
    {
        Character dbCharacter = new()
        {
            Name = "toto",
            UserId = 2,
        };
        ArrangeDb.Characters.Add(dbCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetUserCharacterQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterQuery
        {
            CharacterId = dbCharacter.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task WhenCharacterExistsButNotOwned()
    {
        Character dbCharacter = new()
        {
            Name = "toto",
            UserId = 2,
        };
        ArrangeDb.Characters.Add(dbCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetUserCharacterQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterQuery
        {
            CharacterId = dbCharacter.Id,
            UserId = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
