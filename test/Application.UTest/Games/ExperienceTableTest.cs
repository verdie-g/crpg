using Crpg.Application.Games;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class ExperienceTableTest
    {
        // lvl 1: 3000
        // lvl 2: 18375

        [TestCase(0, 1)]
        [TestCase(2999, 1)]
        [TestCase(3000, 2)]
        [TestCase(3001, 2)]
        [TestCase(20000, 3)]
        public void GetLevelForExperience(int experience, int expectedLevel)
        {
            Assert.AreEqual(expectedLevel, ExperienceTable.GetLevelForExperience(experience));
        }

        [TestCase(1, 0)]
        [TestCase(2, 3000)]
        [TestCase(3, 18375)]
        public void GetExperienceForLevel(int level, int expectedExperience)
        {
            Assert.AreEqual(expectedExperience, ExperienceTable.GetExperienceForLevel(level));
        }
    }
}