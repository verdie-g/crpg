using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RespecializeCharacterCommandTest : TestBase
    {
        private static readonly Constants Constants = new Constants
        {
            MinimumLevel = 1,
            MaximumLevel = 38,
            ExperienceForLevelCoefs = new[] { 0f, 50f, -50f }, // xp = lvl * 10
            AttributePointsPerLevel = 1,
            SkillPointsPerLevel = 1,
            WeaponProficiencyPointsForLevelCoefs = new[] { 100f, 0f }, // wpp = lvl * 100
            RespecializeExperiencePenaltyCoefs = new[] { 0.5f, 0f },
            DefaultStrength = 3,
            DefaultAgility = 3,
        };

        private static readonly ExperienceTable ExperienceTable = new ExperienceTable(Constants);
        private static readonly CharacterService CharacterService = new CharacterService(ExperienceTable, Constants);

        [Test]
        public async Task RespecializeCharacterLevel3ShouldMakeItLevel2()
        {
            var character = new Character
            {
                Generation = 2,
                Level = 3,
                Experience = 150,
                ExperienceMultiplier = 1.1f,
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes
                    {
                        Points = 1,
                        Strength = 4,
                        Agility = 3,
                    },
                    Skills = new CharacterSkills
                    {
                        Points = 1,
                        PowerStrike = 1,
                    },
                    WeaponProficiencies = new CharacterWeaponProficiencies
                    {
                        Points = 67,
                    },
                },
                EquippedItems =
                {
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Head },
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Body },
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon0 },
                },
            };
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var handler = new RespecializeCharacterCommand.Handler(ActDb, Mapper, CharacterService, ExperienceTable, Constants);
            await handler.Handle(new RespecializeCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);

            character = await AssertDb.Characters
                .Include(c => c.EquippedItems)
                .FirstAsync(c => c.Id == character.Id);
            Assert.AreEqual(2, character.Generation);
            Assert.AreEqual(2, character.Level);
            Assert.AreEqual(75, character.Experience);
            Assert.AreEqual(1.1f, character.ExperienceMultiplier);

            Assert.AreEqual(1, character.Statistics.Attributes.Points);
            Assert.AreEqual(3, character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, character.Statistics.Attributes.Agility);
            Assert.AreEqual(1, character.Statistics.Skills.Points);
            Assert.AreEqual(0, character.Statistics.Skills.PowerStrike);
            Assert.AreEqual(200, character.Statistics.WeaponProficiencies.Points);

            Assert.IsEmpty(character.EquippedItems);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfUserDoesntExist()
        {
            var handler = new RespecializeCharacterCommand.Handler(ActDb, Mapper, CharacterService, ExperienceTable, Constants);
            var result = await handler.Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = 2,
                }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterDoesntExist()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var handler = new RespecializeCharacterCommand.Handler(ActDb, Mapper, CharacterService, ExperienceTable, Constants);
            var result = await handler.Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
