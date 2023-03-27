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
        ExperienceForLevelCoefs = new[] { 2f, 0 },
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

        Assert.That(character.Level, Is.EqualTo(1));
        Assert.That(character.Experience, Is.EqualTo(8));
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

        Assert.That(character.Level, Is.EqualTo(1));
        Assert.That(character.Experience, Is.EqualTo(2));
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
        characterService.GiveExperience(character, 6000);

        Assert.That(character.Level, Is.EqualTo(2));
        Assert.That(character.Experience, Is.EqualTo(12002));
        Assert.That(character.Characteristics.Attributes.Points, Is.EqualTo(1));
        Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(1));
        Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(100));
    }

    [Test]
    [Theory]
    public void ResetCharacterStatsShouldResetStats(bool respecialization)
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new()
        {
            Level = 5,
            Class = CharacterClass.ShockInfantry,
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
        Assert.That(character.Class, Is.EqualTo(CharacterClass.Peasant));
        if (respecialization)
        {
            Assert.That(character.Characteristics.Attributes.Points, Is.EqualTo(4));
            Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(6));
            Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(410));
        }
        else
        {
            Assert.That(character.Characteristics.Attributes.Points, Is.Zero);
            Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(2));
            Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(10));
        }

        Assert.That(character.Characteristics.Attributes.Strength, Is.EqualTo(Constants.DefaultStrength));
        Assert.That(character.Characteristics.Attributes.Agility, Is.EqualTo(Constants.DefaultAgility));
        Assert.That(character.Characteristics.Skills.IronFlesh, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerStrike, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerDraw, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerThrow, Is.Zero);
        Assert.That(character.Characteristics.Skills.Athletics, Is.Zero);
        Assert.That(character.Characteristics.Skills.Riding, Is.Zero);
        Assert.That(character.Characteristics.Skills.WeaponMaster, Is.Zero);
        Assert.That(character.Characteristics.Skills.MountedArchery, Is.Zero);
        Assert.That(character.Characteristics.Skills.Shield, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.OneHanded, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.TwoHanded, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Polearm, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Bow, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Throwing, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Crossbow, Is.Zero);
    }

    [Test]
    public void SetDefaultValuesShouldSetDefaultValues()
    {
        CharacterService characterService = new(ExperienceTable, Constants);
        Character character = new() { Level = 2, Experience = 2, ForTournament = false };
        characterService.SetDefaultValuesForCharacter(character);

        Assert.That(character.Level, Is.EqualTo(Constants.MinimumLevel));
        Assert.That(character.Experience, Is.EqualTo(0));
        Assert.That(character.ForTournament, Is.False);
    }
}
