using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities.Characters;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class UpdateEveryCharacterCompetitiveRatingCommandTest : TestBase
{
    [Test]
    public async Task UpdateShouldNotReturnError()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new() { DefaultValue = DefaultValue.Mock };
        competitiveRatingModelMock
            .Setup(m => m.ComputeCompetitiveRating(It.IsAny<CharacterRatingViewModel>()))
            .Returns(500);
        Character character = new()
        {
            Name = "toto",
            UserId = 2,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 100,
            },
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        UpdateEveryCharacterCompetitiveRatingCommand.Handler updateHandler = new(ActDb, Mapper, competitiveRatingModelMock.Object);
        GetUserCharacterRatingQuery.Handler getHandler = new(ActDb, Mapper);
        var putResult = await updateHandler.Handle(new UpdateEveryCharacterCompetitiveRatingCommand
        {
        }, CancellationToken.None);
        var result = await getHandler.Handle(new GetUserCharacterRatingQuery
        {
            CharacterId = character.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.CompetitiveValue, Is.EqualTo(500));
    }
}
