using Crpg.Application.Games;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class ExperienceTableTest
    {
        [TestCase(0, 1)]
        [TestCase(300, 1)]
        [TestCase(600, 2)]
        [TestCase(700, 2)]
        [TestCase(1444, 3)]
        public void GetLevelForExperience(int experience, int expectedLevel)
        {
            Assert.AreEqual(expectedLevel, ExperienceTable.GetLevelForExperience(experience));
        }

        [TestCase(2, 600)]
        [TestCase(3, 1360)]
        public void GetExperienceForLevel(int level, int expectedExperience)
        {
            Assert.AreEqual(expectedExperience, ExperienceTable.GetExperienceForLevel(level));
        }
    }
}