using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class CompetitiveRatingModelTest
{
    [Test]
    [Ignore("Broken for now")]
    public void HigherRatingGivesHigherCompetitiveRating()
    {
        CompetitiveRatingModel competitiveRatingModel = new();
        float cr1 = competitiveRatingModel.ComputeCompetitiveRating(
            new CharacterRating { Value = 100, Deviation = 10 });
        float cr2 = competitiveRatingModel.ComputeCompetitiveRating(
            new CharacterRating { Value = 200, Deviation = 10 });
        Assert.That(cr1, Is.LessThan(cr2));
    }
}
