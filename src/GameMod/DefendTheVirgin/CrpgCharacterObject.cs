using System.Collections.Generic;
using System.Xml;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal sealed class CrpgCharacterObject : BasicCharacterObject
    {
        private readonly int _maxHitPoints;

        public static CrpgCharacterObject New(TextObject name, CharacterSkills skills, Equipment equipment, CrpgConstants constants)
        {
            int strength = skills.GetPropertyValue(CrpgSkills.Strength);
            int ironFlesh = skills.GetPropertyValue(CrpgSkills.IronFlesh);

            int maxHitPoints = (int)(constants.DefaultHealthPoints
                                     + MathHelper.ApplyPolynomialFunction(strength, constants.HealthPointsForStrengthCoefs)
                                     + MathHelper.ApplyPolynomialFunction(ironFlesh, constants.HealthPointsForIronFleshCoefs));

            var mbCharacter = new CrpgCharacterObject(name, maxHitPoints);
            SetCharacterSkills(mbCharacter, skills);
            SetCharacterEquipments(mbCharacter, equipment);
            SetCharacterBodyProperties(mbCharacter);
            return mbCharacter;
        }

        private static void SetCharacterSkills(CrpgCharacterObject mbCharacter, CharacterSkills skills)
        {
            // MBCharacterSkills is expected to be created from an xml node, so reflection needs to be used here.
            var mbCharacterSkills = new MBCharacterSkills();
            ReflectionHelper.SetProperty(mbCharacterSkills, nameof(MBCharacterSkills.Skills), skills);
            mbCharacter.CharacterSkills = mbCharacterSkills;
        }

        private static void SetCharacterEquipments(CrpgCharacterObject mbCharacter, Equipment equipment)
        {
            var mbEquipmentRoster = new MBEquipmentRoster();
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
}
