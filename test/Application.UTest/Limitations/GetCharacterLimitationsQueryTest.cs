using Crpg.Application.Common.Results;
using Crpg.Application.Limitations.Queries;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Limitations;
using NUnit.Framework;

namespace Crpg.Application.UTest.Limitations;

public class GetCharacterLimitationsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterDoesntExist()
    {
        GetCharacterLimitationsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetCharacterLimitationsQuery
        {
            CharacterId = 1,
            UserId = 2,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnCharacterLimitations()
    {
        Character character = new()
        {
            Name = "toto",
            UserId = 2,
            Limitations = new CharacterLimitations(),
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        GetCharacterLimitationsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetCharacterLimitationsQuery
        {
            CharacterId = character.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.IsNull(result.Errors);
        Assert.IsNotNull(result.Data);
    }
}
