using System.Collections.Generic;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal sealed class CrpgCharacterObject : BasicCharacterObject
    {
        private const int BaseHealth = 60;

        public CrpgCharacterObject(TextObject name, BasicCharacterObject characterTemplate, CharacterSkills skills, Equipment equipment)
        {
            Name = name;
            _characterSkills = skills;
            Age = characterTemplate.Age;
            HairTags = characterTemplate.HairTags;
            BeardTags = characterTemplate.BeardTags;
            TattooTags = characterTemplate.TattooTags;
            StaticBodyPropertiesMin = characterTemplate.StaticBodyPropertiesMin;
            StaticBodyPropertiesMax = characterTemplate.StaticBodyPropertiesMax;
            IsFemale = characterTemplate.IsFemale;
            Culture = characterTemplate.Culture;
            DefaultFormationGroup = characterTemplate.DefaultFormationGroup;
            DefaultFormationClass = characterTemplate.DefaultFormationClass;
            FormationPositionPreference = characterTemplate.FormationPositionPreference;

            // Those fields are private for some reason. Don't do this at home.
            var equipments = new List<Equipment> { equipment };
            ReflectionHelper.SetField(this, "_equipments", equipments);
            ReflectionHelper.SetField(this, "_equipmentsAsReadOnlyList", equipments.GetReadOnlyList());
            ReflectionHelper.SetProperty(this, nameof(IsSoldier), true);
            ReflectionHelper.SetField(this, "_isBasicHero", false);

            var dynamicBodyPropertiesMin = ReflectionHelper.GetField(characterTemplate, "_dynamicBodyPropertiesMin");
            ReflectionHelper.SetField(this, "_dynamicBodyPropertiesMin", dynamicBodyPropertiesMin);

            var dynamicBodyPropertiesMax = ReflectionHelper.GetField(characterTemplate, "_dynamicBodyPropertiesMax");
            ReflectionHelper.SetField(this, "_dynamicBodyPropertiesMax", dynamicBodyPropertiesMax);
        }

        public override int MaxHitPoints() => BaseHealth + _characterSkills.GetPropertyValue(CrpgSkills.Strength)
                                                         + _characterSkills.GetPropertyValue(CrpgSkills.IronFlesh) * 2;
    }
}
