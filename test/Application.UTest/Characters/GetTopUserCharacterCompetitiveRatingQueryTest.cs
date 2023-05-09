using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetTopUserCharacterCompetitiveRatingQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterRatingDoesntExist()
    {
        GetTopUserCharactersByCompetitiveRatingQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetTopUserCharactersByCompetitiveRatingQuery
        {
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnCharacterRating()
    {
        Character character1 = new()
        {
            Name = "toto",
            UserId = 2,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
            },
        };
        Character character2 = new()
        {
            Name = "lolo",
            UserId = 3,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
            },
        };
        Character character3 = new()
        {
            Name = "popo",
            UserId = 4,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
            },
        };
        ArrangeDb.Characters.Add(character1);
        ArrangeDb.Characters.Add(character2);
        ArrangeDb.Characters.Add(character3);
        await ArrangeDb.SaveChangesAsync();

        GetTopUserCharactersByCompetitiveRatingQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetTopUserCharactersByCompetitiveRatingQuery
        {
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data!.ToList().Count, Is.EqualTo(3));
    }
}
