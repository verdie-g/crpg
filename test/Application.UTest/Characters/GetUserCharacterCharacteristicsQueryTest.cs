using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserCharacterCharacteristicsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterDoesntExist()
    {
        GetUserCharacterCharacteristicsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterCharacteristicsQuery
        {
            CharacterId = 1,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldReturnCharacterCharacteristics()
    {
        Character character = new()
        {
            Name = "toto",
            UserId = 2,
            Characteristics = new CharacterCharacteristics(),
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        GetUserCharacterCharacteristicsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterCharacteristicsQuery
        {
            CharacterId = character.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
    }
}
