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

        public CrpgCharacterObject(TextObject name, CharacterSkills skills)
        {
            Name = name;
            _characterSkills = skills;

            // Those fields are private for some reason. Don't do this at home.
            ReflectionHelper.SetProperty(this, nameof(IsSoldier), true);
        }

        public override int MaxHitPoints() => BaseHealth + _characterSkills.GetPropertyValue(CrpgSkills.Strength)
                                                         + _characterSkills.GetPropertyValue(CrpgSkills.IronFlesh) * 2;
    }
}
