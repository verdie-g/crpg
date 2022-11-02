using Crpg.Application.Common;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Services;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ExperienceTableTest
{
    private ExperienceTable ExperienceTable = default!;
    private Constants constants = default!;
    [SetUp]
    public void SetUp()
    {
        FileConstantsSource source = new();
        constants = source.LoadConstants();
        ExperienceTable = new ExperienceTable(constants);
    }

    [TestCase(0, 0)]
    [TestCase(3539, 1)]
    [TestCase(4420824, 30)]
    [TestCase(8841648, 31)]
    public void GetLevelForExperience(int experience, int expectedLevel)
    {
        Assert.AreEqual(expectedLevel, ExperienceTable.GetLevelForExperience(experience));
    }

    [TestCase(1, 2000)]
    [TestCase(2, 5000)]
    [TestCase(3, 8000)]
    public void GetExperienceForLevel(int level, int expectedExperience)
    {
        Assert.AreEqual(expectedExperience, ExperienceTable.GetExperienceForLevel(level));
    }

        [Test]
    public void ExperienceShouldBeExponentialAfterLevel30()
    {
        int xpLastLevel = ExperienceTable.GetExperienceForLevel(30);
        for (int lvl = 31; lvl <= constants.MaximumLevel; lvl += 1)
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
        for (int lvl = constants.MinimumLevel; lvl <= constants.MaximumLevel; lvl += 1)
        {
            int xp = ExperienceTable.GetExperienceForLevel(lvl);
            Assert.Greater(xp, xpLastLevel, "Experience for lvl {0} should be greater than for lvl {1}", lvl, lvl - 1);
            xpLastLevel = xp;
        }
    }
}
