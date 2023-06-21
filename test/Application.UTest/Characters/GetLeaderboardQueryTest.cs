using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserLeaderboardQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User orle = new User()
        {
            Name = "Orle",
        };

        User takeo = new User()
        {
            Name = "Takeo",
        };

        User namidaka = new User()
        {
            Name = "Namidaka",
        };
        Character orleCharacter = new()
        {
            Name = "shielder",
            UserId = orle.Id,
            User = orle,
            Class = CharacterClass.Infantry,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1800,
            },
        };
        Character takeoCharacter = new()
        {
            Name = "2h",
            UserId = takeo.Id,
            User = takeo,
            Class = CharacterClass.ShockInfantry,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1500,
            },
        };

        Character namidakaCharacter = new()
        {
            Name = "Nami Legodaklas",
            UserId = namidaka.Id,
            User = namidaka,
            Class = CharacterClass.Archer,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1400,
            },
        };
        ArrangeDb.Users.Add(orle);
        ArrangeDb.Users.Add(takeo);
        ArrangeDb.Users.Add(namidaka);
        ArrangeDb.Characters.Add(takeoCharacter);
        ArrangeDb.Characters.Add(orleCharacter);
        ArrangeDb.Characters.Add(namidakaCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetLeaderboardQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetLeaderboardQuery
        {
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.First().Class, Is.EqualTo(CharacterClass.Infantry));
        Assert.That(result.Data!.Last().Class, Is.EqualTo(CharacterClass.Archer));
    }
}
