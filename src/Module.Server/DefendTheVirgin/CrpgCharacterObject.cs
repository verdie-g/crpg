using System.Xml;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.DefendTheVirgin;

internal sealed class CrpgCharacterObject : BasicCharacterObject
{
    private readonly int _maxHitPoints;

    public override bool IsHero => true; // Spawning a non-hero gives random item modifiers.

    public static CrpgCharacterObject New(TextObject name, CharacterSkills skills, Equipment equipment, CrpgConstants constants)
    {
        int strength = skills.GetPropertyValue(CrpgSkills.Strength);
        int ironFlesh = skills.GetPropertyValue(CrpgSkills.IronFlesh);

        int maxHitPoints = (int)(constants.DefaultHealthPoints
                                 + MathHelper.ApplyPolynomialFunction(strength, constants.HealthPointsForStrengthCoefs)
                                 + MathHelper.ApplyPolynomialFunction(ironFlesh, constants.HealthPointsForIronFleshCoefs));

        CrpgCharacterObject mbCharacter = new(name, maxHitPoints);
        SetCharacterSkills(mbCharacter, skills);
        SetCharacterEquipments(mbCharacter, equipment);
        SetCharacterBodyProperties(mbCharacter);
        return mbCharacter;
    }

    private static void SetCharacterSkills(CrpgCharacterObject mbCharacter, CharacterSkills skills)
    {
        // MBCharacterSkills is expected to be created from an xml node, so reflection needs to be used here.
        MBCharacterSkills mbCharacterSkills = new();
        ReflectionHelper.SetProperty(mbCharacterSkills, nameof(MBCharacterSkills.Skills), skills);
        mbCharacter.CharacterSkills = mbCharacterSkills;
    }

    private static void SetCharacterEquipments(CrpgCharacterObject mbCharacter, Equipment equipment)
    {
        MBEquipmentRoster mbEquipmentRoster = new();
        ReflectionHelper.SetField(mbEquipmentRoster, "_equipments", new List<Equipment> { equipment });
        ReflectionHelper.SetField(mbCharacter, "_equipmentRoster", mbEquipmentRoster);
    }

    private static void SetCharacterBodyProperties(CrpgCharacterObject mbCharacter)
    {
        var bodyProperties = MBObjectManager.Instance.GetObject<MBBodyProperty>("villager_battania");
        mbCharacter.BodyPropertyRange = MBObjectManager.Instance.CreateObject<MBBodyProperty>("whatever");
        mbCharacter.BodyPropertyRange.Init(bodyProperties.BodyPropertyMin, bodyProperties.BodyPropertyMax);
    }

    private static StaticBodyProperties StaticBodyPropertiesFromString(string bodyPropertiesKey)
    {
        XmlDocument doc = new();
        doc.LoadXml($"<BodyProperties key=\"{bodyPropertiesKey}\" />");
        if (!StaticBodyProperties.FromXmlNode(doc.DocumentElement, out var staticBodyProperties))
        {
            // TODO: log warning.
        }

        return staticBodyProperties;
    }

    private CrpgCharacterObject(TextObject name, int maxHitPoints)
    {
        _basicName = name;
        _maxHitPoints = maxHitPoints;
    }

    public override int MaxHitPoints() => _maxHitPoints;
}
