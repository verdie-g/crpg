using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ExperienceTableTest
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MaximumLevel = 38,
        ExperienceForLevelCoefs = new[] { 13f, 200f },
    };

    private static readonly ExperienceTable ExperienceTable = new(Constants);

    [TestCase(500, 2)]
    [TestCase(4000, 11)]
    [TestCase(5000000, 30)]
    [TestCase(9000000, 31)]
    public void GetLevelForExperience(int experience, int expectedLevel)
    {
        Assert.That(ExperienceTable.GetLevelForExperience(experience), Is.EqualTo(expectedLevel));
    }

    [TestCase(2, 388)]
    [TestCase(3, 777)]
    [TestCase(4, 1166)]
    [TestCase(30, 4420824)]
    public void GetExperienceForLevel(int level, int expectedExperience)
    {
        Assert.That(ExperienceTable.GetExperienceForLevel(level), Is.EqualTo(expectedExperience));
    }

    [Test]
    public void ExperienceShouldBeExponentialAfterLevel30()
    {
        int xpLastLevel = ExperienceTable.GetExperienceForLevel(30);
        for (int lvl = 31; lvl <= Constants.MaximumLevel; lvl += 1)
        {
            int xp = ExperienceTable.GetExperienceForLevel(lvl);
            Assert.That(xp, Is.EqualTo(2 * xpLastLevel));
            xpLastLevel = xp;
        }
    }

    [Test]
    public void ExperienceTableShouldBeIncreasing()
    {
        int xpLastLevel = -1;
        for (int lvl = Constants.MinimumLevel; lvl <= Constants.MaximumLevel; lvl += 1)
        {
            int xp = ExperienceTable.GetExperienceForLevel(lvl);
            Assert.That(xp, Is.GreaterThan(xpLastLevel), "Experience for lvl {0} should be greater than for lvl {1}", lvl, lvl - 1);
            xpLastLevel = xp;
        }
    }
}
