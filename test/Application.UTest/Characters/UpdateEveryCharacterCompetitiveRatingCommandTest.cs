using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class UpdateEveryCharacterCompetitiveRatingCommandTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Character character0 = new();
        Character character1 = new();
        ArrangeDb.Characters.AddRange(character0, character1);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICompetitiveRatingModel> competitiveRatingModel = new();
        UpdateEveryCharacterCompetitiveRatingCommand.Handler handler = new(ActDb, competitiveRatingModel.Object);
        await handler.Handle(new UpdateEveryCharacterCompetitiveRatingCommand(), CancellationToken.None);

        competitiveRatingModel.Verify(m => m.ComputeCompetitiveRating(It.IsAny<CharacterRating>()), Times.Exactly(2));
    }
}
