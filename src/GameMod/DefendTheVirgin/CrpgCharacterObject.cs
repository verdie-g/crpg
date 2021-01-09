using System.Collections.Generic;
using System.Xml;
using Crpg.Common.Helpers;
using Crpg.GameMod.Api.Models.Characters;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal sealed class CrpgCharacterObject : BasicCharacterObject
    {
        private readonly int _maxHitPoints;

        public static CrpgCharacterObject New(TextObject name, CharacterSkills skills, Equipment equipment,
            string bodyProperties, CrpgCharacterGender gender, CrpgConstants constants)
        {
            int strength = skills.GetPropertyValue(CrpgSkills.Strength);
            int ironFlesh = skills.GetPropertyValue(CrpgSkills.IronFlesh);

            int maxHitPoints = (int)(constants.DefaultHealthPoints
                                     + MathHelper.ApplyPolynomialFunction(strength, constants.HealthPointsForStrengthCoefs)
                                     + MathHelper.ApplyPolynomialFunction(ironFlesh, constants.HealthPointsForIronFleshCoefs));

            var mbCharacter = new CrpgCharacterObject(name, skills, maxHitPoints);
            SetCharacterBodyProperties(mbCharacter, bodyProperties, gender);
            mbCharacter.InitializeEquipmentsOnLoad(new List<Equipment> { equipment });
            return mbCharacter;
        }

        private static void SetCharacterBodyProperties(CrpgCharacterObject mbCharacter, string bodyPropertiesKey, CrpgCharacterGender gender)
        {
            var dynamicBodyProperties = new DynamicBodyProperties { Age = 20 }; // Age chosen arbitrarily.
            var staticBodyProperties = StaticBodyPropertiesFromString(bodyPropertiesKey);
            var bodyProperties = new BodyProperties(dynamicBodyProperties, staticBodyProperties);
            bodyProperties = bodyProperties.ClampForMultiplayer(); // Clamp height and set Build & Weight.
            mbCharacter.UpdatePlayerCharacterBodyProperties(bodyProperties, gender == CrpgCharacterGender.Female);
            ReflectionHelper.SetField(mbCharacter, "_dynamicBodyPropertiesMin", mbCharacter.GetBodyPropertiesMax().DynamicProperties);
        }

        private static StaticBodyProperties StaticBodyPropertiesFromString(string bodyPropertiesKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml($"<BodyProperties key=\"{bodyPropertiesKey}\" />");
            if (!StaticBodyProperties.FromXmlNode(doc.DocumentElement, out var staticBodyProperties))
            {
                // TODO: log warning.
            }

            return staticBodyProperties;
        }

        private CrpgCharacterObject(TextObject name, CharacterSkills skills, int maxHitPoints)
        {
            Name = name;
            _characterSkills = skills;
            _maxHitPoints = maxHitPoints;

            // Those fields are private for some reason. Don't do this at home.
            ReflectionHelper.SetProperty(this, nameof(IsSoldier), true);
        }

        public override int MaxHitPoints() => _maxHitPoints;
    }
}
