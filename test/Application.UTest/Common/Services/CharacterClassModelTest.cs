using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class CharacterClassModelTest
{
    [TestCaseSource(nameof(TestResolveCharacterClassSource))]
    public void TestResolveCharacterClass(CharacterClass expectedClass, CharacterCharacteristics stats)
    {
        CharacterClassResolver resolver = new();
        CharacterClass actualClass = resolver.ResolveCharacterClass(stats);
        Assert.AreEqual(expectedClass, actualClass);
    }

    private static readonly object[][] TestResolveCharacterClassSource =
    {
        new object[]
        {
            CharacterClass.Cavalry,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills { Riding = 5 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Polearm = 120 },
            },
        },
        new object[]
        {
            CharacterClass.MountedArcher,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills { Riding = 4 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Crossbow = 100 },
            },
        },
        new object[]
        {
            CharacterClass.MountedArcher,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills { Riding = 3 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Crossbow = 80 },
            },
        },
        new object[]
        {
            CharacterClass.Skirmisher,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills(),
                WeaponProficiencies = new CharacterWeaponProficiencies { Throwing = 60 },
            },
        },
        new object[]
        {
            CharacterClass.Crossbowman,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills(),
                WeaponProficiencies = new CharacterWeaponProficiencies { Crossbow = 70 },
            },
        },
        new object[]
        {
            CharacterClass.Archer,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills(),
                WeaponProficiencies = new CharacterWeaponProficiencies { Bow = 50 },
            },
        },
        new object[]
        {
            CharacterClass.Infantry,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills { Shield = 4 },
                WeaponProficiencies = new CharacterWeaponProficiencies(),
            },
        },
        new object[]
        {
            CharacterClass.ShockInfantry,
            new CharacterCharacteristics
            {
                Skills = new CharacterSkills(),
                WeaponProficiencies = new CharacterWeaponProficiencies { TwoHanded = 120 },
            },
        },
    };
}
