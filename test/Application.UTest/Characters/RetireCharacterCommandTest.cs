using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RetireCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var character = new Character
            {
                Generation = 2,
                Level = 31,
                Experience = 42424424,
                ExperienceMultiplier = 1.03f,
                EquippedItems =
                {
                    new EquippedItem { Slot = ItemSlot.Head },
                    new EquippedItem { Slot = ItemSlot.Hand },
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
            };
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            await new RetireCharacterCommand.Handler(ActDb, Mapper).Handle(new RetireCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);

            character = await AssertDb.Characters
                .Include(c => c.User)
                .Include(c => c.EquippedItems)
                .FirstAsync(c => c.Id == character.Id);
            Assert.AreEqual(3, character.Generation);
            Assert.AreEqual(1, character.Level);
            Assert.AreEqual(0, character.Experience);
            Assert.AreEqual(1.06f, character.ExperienceMultiplier);
            Assert.AreEqual(2, character.User!.HeirloomPoints);

            Assert.AreEqual(0, character.Statistics.Attributes.Points);
            Assert.AreEqual(3, character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, character.Statistics.Attributes.Agility);

            Assert.AreEqual(0, character.Statistics.Skills.Points);
            Assert.AreEqual(0, character.Statistics.Skills.IronFlesh);
            Assert.AreEqual(0, character.Statistics.Skills.PowerStrike);
            Assert.AreEqual(0, character.Statistics.Skills.PowerDraw);
            Assert.AreEqual(0, character.Statistics.Skills.PowerThrow);
            Assert.AreEqual(0, character.Statistics.Skills.Athletics);
            Assert.AreEqual(0, character.Statistics.Skills.Riding);
            Assert.AreEqual(0, character.Statistics.Skills.WeaponMaster);
            Assert.AreEqual(0, character.Statistics.Skills.HorseArchery);
            Assert.AreEqual(0, character.Statistics.Skills.Shield);

            Assert.AreEqual(57, character.Statistics.WeaponProficiencies.Points);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.OneHanded);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.TwoHanded);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.Polearm);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.Bow);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.Throwing);
            Assert.AreEqual(0, character.Statistics.WeaponProficiencies.Crossbow);

            Assert.IsEmpty(character.EquippedItems);
        }

        [Test]
        public async Task NotFoundIfUserDoesntExist()
        {
            var result = await new RetireCharacterCommand.Handler(ActDb, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = 1,
                    UserId = 2,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task NotFoundIfCharacterDoesntExist()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var result = await new RetireCharacterCommand.Handler(ActDb, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task BadRequestIfLevelTooLow()
        {
            var character = ArrangeDb.Characters.Add(new Character
            {
                Level = 30,
                User = new User(),
            });
            await ArrangeDb.SaveChangesAsync();

            var result = await new RetireCharacterCommand.Handler(ActDb, Mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = character.Entity.Id,
                    UserId = character.Entity.UserId,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterLevelRequirementNotMet, result.Errors![0].Code);
        }
    }
}
