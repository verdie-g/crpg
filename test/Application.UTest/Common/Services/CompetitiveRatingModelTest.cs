using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class CompetitiveRatingModelTest
{
    [Test]
    public void HigherRatingGivesHigherCompetitiveRating()
    {
        CompetitiveRatingModel competitiveRatingModel = new();
        float cr1 = competitiveRatingModel.ComputeCompetitiveRating(
            new CharacterRating { Value = 100, Deviation = 500 });
        float cr2 = competitiveRatingModel.ComputeCompetitiveRating(
            new CharacterRating { Value = 200, Deviation = 500 });
        Assert.That(cr1, Is.LessThan(cr2));
    }
}
