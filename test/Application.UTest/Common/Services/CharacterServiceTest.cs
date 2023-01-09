using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class CharacterServiceTest
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MaximumLevel = 38,
        ExperienceForLevelCoefs = new[] { 5.65f, 150000f }, // xp = lvl * 10
        DefaultAttributePoints = 0,
        AttributePointsPerLevel = 1,
        DefaultSkillPoints = 2,
        SkillPointsPerLevel = 1,
        WeaponProficiencyPointsForLevelCoefs = new[] { 100f, -90f }, // wpp = lvl * 100 - 90
        DefaultStrength = 3,
        DefaultAgility = 3,
        DefaultGeneration = 1,
    };

    private static readonly ExperienceTable ExperienceTable = new(Constants);

    [Test]
    public void GiveExperienceShouldGiveExperience()
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = false,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 3);

        Assert.AreEqual(1, character.Level);
        Assert.AreEqual(8, character.Experience);
    }

    [Test]
    public void GiveExperienceShouldntGiveExperienceIfTournamentCharacter()
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = true,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 3);

        Assert.AreEqual(1, character.Level);
        Assert.AreEqual(2, character.Experience);
    }

    [Test]
    public void GiveExperienceShouldMakeCharacterLevelUpIfEnoughExperience()
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = false,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 2500);

        Assert.AreEqual(2, character.Level);
        Assert.AreEqual(5002, character.Experience);
        Assert.AreEqual(1, character.Characteristics.Attributes.Points);
        Assert.AreEqual(1, character.Characteristics.Skills.Points);
        Assert.AreEqual(100, character.Characteristics.WeaponProficiencies.Points);
    }

    [Test]
    [Theory]
    public void ResetCharacterStatsShouldResetStats(bool respecialization)
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new()
        {
            Level = 5,
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes
                {
                    Points = 1,
                    Agility = 5,
                    Strength = 7,
                },
                Skills = new CharacterSkills
                {
                    Points = 2,
                    IronFlesh = 3,
                    PowerStrike = 4,
                    PowerDraw = 5,
                    PowerThrow = 6,
                    Athletics = 7,
                    Riding = 8,
                    WeaponMaster = 9,
                    MountedArchery = 10,
                    Shield = 11,
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
        };

        characterService.ResetCharacterCharacteristics(character, respecialization);
        if (respecialization)
        {
            Assert.AreEqual(4, character.Characteristics.Attributes.Points);
            Assert.AreEqual(6, character.Characteristics.Skills.Points);
            Assert.AreEqual(410, character.Characteristics.WeaponProficiencies.Points);
        }
        else
        {
            Assert.Zero(character.Characteristics.Attributes.Points);
            Assert.AreEqual(2, character.Characteristics.Skills.Points);
            Assert.AreEqual(10, character.Characteristics.WeaponProficiencies.Points);
        }

        Assert.AreEqual(Constants.DefaultStrength, character.Characteristics.Attributes.Strength);
        Assert.AreEqual(Constants.DefaultAgility, character.Characteristics.Attributes.Agility);
        Assert.Zero(character.Characteristics.Skills.IronFlesh);
        Assert.Zero(character.Characteristics.Skills.PowerStrike);
        Assert.Zero(character.Characteristics.Skills.PowerDraw);
        Assert.Zero(character.Characteristics.Skills.PowerThrow);
        Assert.Zero(character.Characteristics.Skills.Athletics);
        Assert.Zero(character.Characteristics.Skills.Riding);
        Assert.Zero(character.Characteristics.Skills.WeaponMaster);
        Assert.Zero(character.Characteristics.Skills.MountedArchery);
        Assert.Zero(character.Characteristics.Skills.Shield);
        Assert.Zero(character.Characteristics.WeaponProficiencies.OneHanded);
        Assert.Zero(character.Characteristics.WeaponProficiencies.TwoHanded);
        Assert.Zero(character.Characteristics.WeaponProficiencies.Polearm);
        Assert.Zero(character.Characteristics.WeaponProficiencies.Bow);
        Assert.Zero(character.Characteristics.WeaponProficiencies.Throwing);
        Assert.Zero(character.Characteristics.WeaponProficiencies.Crossbow);
    }

    [Test]
    public void SetDefaultValuesShouldSetDefaultValues()
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new() { Level = 2, Experience = 2, ForTournament = false };
        characterService.SetDefaultValuesForCharacter(character);

        Assert.AreEqual(Constants.MinimumLevel, character.Level);
        Assert.AreEqual(0, character.Experience);
        Assert.IsFalse(character.ForTournament);
    }
}
