using Crpg.Application.Common;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Services;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ExperienceTableTest
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MaximumLevel = 38,
        ExperienceForLevelCoefs = new[] { 5.65f, 150000f }, // 50 xp for each level
    };
    private static readonly ExperienceTable ExperienceTable = new(Constants);
    [TestCase(500, 1)]
    [TestCase(4000, 2)]
    [TestCase(5000000, 30)]
    [TestCase(9000000, 31)]
    public void GetLevelForExperience(int experience, int expectedLevel)
    {
        Assert.AreEqual(expectedLevel, ExperienceTable.GetLevelForExperience(experience));
    }

    [TestCase(2, 3538)]
    [TestCase(3, 7078)]
    [TestCase(4, 10627)]
    [TestCase(30, 4420824)]
    public void GetExperienceForLevel(int level, int expectedExperience)
    {
        Assert.AreEqual(expectedExperience, ExperienceTable.GetExperienceForLevel(level));
    }

    [Test]
    public void ExperienceShouldBeExponentialAfterLevel30()
    {
        int xpLastLevel = ExperienceTable.GetExperienceForLevel(30);
        for (int lvl = 31; lvl <= Constants.MaximumLevel; lvl += 1)
        {
            int xp = ExperienceTable.GetExperienceForLevel(lvl);
            Assert.AreEqual(2 * xpLastLevel, xp);
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
            Assert.Greater(xp, xpLastLevel, "Experience for lvl {0} should be greater than for lvl {1}", lvl, lvl - 1);
            xpLastLevel = xp;
        }
    }
}
