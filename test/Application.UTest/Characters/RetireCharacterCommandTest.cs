using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RetireCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var character = Db.Characters.Add(new Character
            {
                Experience = 42424424,
                Level = 31,
                ExperienceMultiplier = 1.03f,
                Items = new CharacterItems
                {
                    HeadItem = new Item(),
                    CapeItem = new Item(),
                    BodyItem = new Item(),
                    HandItem = new Item(),
                    LegItem = new Item(),
                    HorseHarnessItem = new Item(),
                    HorseItem = new Item(),
                    Weapon1Item = new Item(),
                    Weapon2Item = new Item(),
                    Weapon3Item = new Item(),
                    Weapon4Item = new Item(),
                },
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes
                    {
                        Points = 1,
                        Strength = 18,
                        Agility = 24,
                    },
                    Skills = new CharacterSkills
                    {
                        Points = 2,
                        IronFlesh = 1,
                        PowerStrike = 2,
                        PowerDraw = 3,
                        PowerThrow = 4,
                        Athletics = 5,
                        Riding = 6,
                        WeaponMaster = 7,
                        HorseArchery = 8,
                        Shield = 9,
                    },
                    WeaponProficiencies = new CharacterWeaponProficiencies
                    {
                        Points = 1,
                        OneHanded = 2,
                        TwoHanded = 3,
                        Polearm = 4,
                        Bow = 5,
                        Throwing = 6,
                        Crossbow = 7,
                    },
                },
                User = new User
                {
                    HeirloomPoints = 1,
                }
            });
            await Db.SaveChangesAsync();

            await new RetireCharacterCommand.Handler(Db, Mapper).Handle(new RetireCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
            }, CancellationToken.None);

            Assert.AreEqual(1, character.Entity.Level);
            Assert.AreEqual(0, character.Entity.Experience);
            Assert.AreEqual(1.06f, character.Entity.ExperienceMultiplier);
            Assert.AreEqual(2, character.Entity.User!.HeirloomPoints);

            Assert.AreEqual(0, character.Entity.Statistics.Attributes.Points);
            Assert.AreEqual(3, character.Entity.Statistics.Attributes.Strength);
            Assert.AreEqual(3, character.Entity.Statistics.Attributes.Agility);

            Assert.AreEqual(0, character.Entity.Statistics.Skills.Points);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.IronFlesh);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.PowerStrike);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.PowerDraw);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.PowerThrow);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.Athletics);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.Riding);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.WeaponMaster);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.HorseArchery);
            Assert.AreEqual(0, character.Entity.Statistics.Skills.Shield);

            Assert.AreEqual(57, character.Entity.Statistics.WeaponProficiencies.Points);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.OneHanded);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.TwoHanded);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.Polearm);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.Bow);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.Throwing);
            Assert.AreEqual(0, character.Entity.Statistics.WeaponProficiencies.Crossbow);

            Assert.Null(character.Entity.Items.HeadItem);
            Assert.Null(character.Entity.Items.CapeItem);
            Assert.Null(character.Entity.Items.BodyItem);
            Assert.Null(character.Entity.Items.HandItem);
            Assert.Null(character.Entity.Items.LegItem);
            Assert.Null(character.Entity.Items.HorseHarnessItem);
            Assert.Null(character.Entity.Items.HorseItem);
            Assert.Null(character.Entity.Items.Weapon1Item);
            Assert.Null(character.Entity.Items.Weapon2Item);
            Assert.Null(character.Entity.Items.Weapon3Item);
            Assert.Null(character.Entity.Items.Weapon4Item);
        }

        [Test]
        public void NotFoundIfUserDoesntExist()
        {
            Assert.ThrowsAsync<NotFoundException>(() => new RetireCharacterCommand.Handler(Db, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = 1,
                    UserId = 2,
                }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundIfCharacterDoesntExist()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new RetireCharacterCommand.Handler(Db, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task BadRequestIfLevelTooLow()
        {
            var character = Db.Characters.Add(new Character
            {
                Level = 30,
                User = new User(),
            });
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<BadRequestException>(() => new RetireCharacterCommand.Handler(Db, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = character.Entity.Id,
                    UserId = character.Entity.UserId,
                }, CancellationToken.None));
        }
    }
}