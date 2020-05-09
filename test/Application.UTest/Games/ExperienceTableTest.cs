using Crpg.Application.Games;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class ExperienceTableTest
    {
        [TestCase(0, 1)]
        [TestCase(2997, 1)]
        [TestCase(2998, 2)]
        [TestCase(3000, 2)]
        [TestCase(20000, 3)]
        public void GetLevelForExperience(int experience, int expectedLevel)
        {
            Assert.AreEqual(expectedLevel, ExperienceTable.GetLevelForExperience(experience));
        }

        [TestCase(1, 0)]
        [TestCase(2, 2998)]
        [TestCase(3, 18373)]
        public void GetExperienceForLevel(int level, int expectedExperience)
        {
            Assert.AreEqual(expectedExperience, ExperienceTable.GetExperienceForLevel(level));
        }
    }
}