using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RespecializeCharacterCommandTest : TestBase
    {
        [Test]
        public async Task RespecializeCharacterLevel3ShouldMakeItLevel2()
        {
            var character = new Character
            {
                Generation = 2,
                Level = 3,
                Experience = 20000,
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
                Items = new CharacterItems
                {
                    HeadItem = new Item(),
                    BodyItem = new Item(),
                    Weapon1Item = new Item(),
                },
            };
            Db.Add(character);
            await Db.SaveChangesAsync();

            await new RespecializeCharacterCommand.Handler(Db, Mapper).Handle(new RespecializeCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);

            Assert.AreEqual(2, character.Generation);
            Assert.AreEqual(2, character.Level);
            Assert.AreEqual(10000, character.Experience);
            Assert.AreEqual(1.1f, character.ExperienceMultiplier);

            Assert.AreEqual(1, character.Statistics.Attributes.Points);
            Assert.AreEqual(3, character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, character.Statistics.Attributes.Agility);
            Assert.AreEqual(1, character.Statistics.Skills.Points);
            Assert.AreEqual(0, character.Statistics.Skills.PowerStrike);
            Assert.AreEqual(62, character.Statistics.WeaponProficiencies.Points);

            Assert.Null(character.Items.HeadItem);
            Assert.Null(character.Items.BodyItem);
            Assert.Null(character.Items.Weapon1Item);
        }

        [Test]
        public void ShouldThrowNotFoundIfUserDoesntExist()
        {
            Assert.ThrowsAsync<NotFoundException>(() => new RespecializeCharacterCommand.Handler(Db, Mapper).Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = 2,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterDoesntExist()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new RespecializeCharacterCommand.Handler(Db, Mapper).Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }
    }
}