using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Crpg.Module.Common;

internal sealed class CrpgCharacterObject : BasicCharacterObject
{
    private readonly int _maxHitPoints;

    public override bool IsHero => true; // Spawning a non-hero gives random item modifiers.

    public static CrpgCharacterObject New(TextObject name, CharacterSkills skills, CrpgConstants constants)
    {
        int strength = skills.GetPropertyValue(CrpgSkills.Strength);
        int ironFlesh = skills.GetPropertyValue(CrpgSkills.IronFlesh);

        int maxHitPoints = (int)(constants.DefaultHealthPoints
                                 + MathHelper.ApplyPolynomialFunction(strength, constants.HealthPointsForStrengthCoefs)
                                 + MathHelper.ApplyPolynomialFunction(ironFlesh, constants.HealthPointsForIronFleshCoefs));

        CrpgCharacterObject mbCharacter = new(name, maxHitPoints);
        SetCharacterSkills(mbCharacter, skills);
        return mbCharacter;
    }

    private static void SetCharacterSkills(CrpgCharacterObject mbCharacter, CharacterSkills skills)
    {
        // MBCharacterSkills is expected to be created from an xml node, so reflection needs to be used here.
        MBCharacterSkills mbCharacterSkills = new();
        ReflectionHelper.SetProperty(mbCharacterSkills, nameof(MBCharacterSkills.Skills), skills);
        mbCharacter.CharacterSkills = mbCharacterSkills;
    }

    private CrpgCharacterObject(TextObject name, int maxHitPoints)
    {
        _basicName = name;
        _maxHitPoints = maxHitPoints;
    }

    public override int MaxHitPoints() => _maxHitPoints;
}
