using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserCharacterStatisticsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterDoesntExist()
    {
        GetUserCharacterStatisticsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterStatisticsQuery
        {
            CharacterId = 1,
            UserId = 2,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnCharacterStatistics()
    {
        Character character = new()
        {
            Name = "toto",
            UserId = 2,
            Statistics = new CharacterStatistics(),
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        GetUserCharacterStatisticsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharacterStatisticsQuery
        {
            CharacterId = character.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.IsNull(result.Errors);
        Assert.IsNotNull(result.Data);
    }
}
