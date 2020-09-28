using System.Collections.Generic;
using TaleWorlds.Core;

namespace Crpg.GameMod.Common
{
    internal class CrpgSkillList : SkillList
    {
        public override IEnumerable<SkillObject> GetSkillList()
        {
            yield return CrpgSkills.Strength;
            yield return CrpgSkills.Agility;
            yield return CrpgSkills.IronFlesh;
            yield return CrpgSkills.PowerStrike;
            yield return CrpgSkills.PowerDraw;
            yield return CrpgSkills.PowerThrow;
            yield return DefaultSkills.Athletics;
            yield return DefaultSkills.Riding;
            yield return CrpgSkills.WeaponMaster;
            yield return CrpgSkills.HorseArchery;
            yield return CrpgSkills.Shield;
            yield return DefaultSkills.OneHanded;
            yield return DefaultSkills.TwoHanded;
            yield return DefaultSkills.Polearm;
            yield return DefaultSkills.Bow;
            yield return DefaultSkills.Crossbow;
            yield return DefaultSkills.Throwing;
        }
    }
}
