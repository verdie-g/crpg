using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Crpg.GameMod.Common
{
    public class CrpgSkills
    {
        public static SkillObject Strength { get; private set; } = default!;
        public static SkillObject Agility { get; private set; } = default!;
        public static SkillObject IronFlesh { get; private set; } = default!;
        public static SkillObject PowerStrike { get; private set; } = default!;
        public static SkillObject PowerDraw { get; private set; } = default!;
        public static SkillObject PowerThrow { get; private set; } = default!;
        public static SkillObject WeaponMaster { get; private set; } = default!;
        public static SkillObject HorseArchery { get; private set; } = default!;
        public static SkillObject Shield { get; private set; } = default!;

        public static void Initialize(Game game)
        {
            Strength = InitializeSkill(game, nameof(Strength), "Strength", "");
            Agility = InitializeSkill(game, nameof(Agility), "Agility", "");
            IronFlesh = InitializeSkill(game, nameof(IronFlesh), "Iron Flesh", "");
            PowerStrike = InitializeSkill(game, nameof(PowerStrike), "Power Strike", "");
            PowerDraw = InitializeSkill(game, nameof(PowerDraw), "Power Draw", "");
            PowerThrow = InitializeSkill(game, nameof(PowerThrow), "Power Throw", "");
            WeaponMaster = InitializeSkill(game, nameof(WeaponMaster), "Weapon Master", "");
            HorseArchery = InitializeSkill(game, nameof(HorseArchery), "Horse Archery", "");
            Shield = InitializeSkill(game, nameof(Shield), "Shield", "");
        }

        private static SkillObject InitializeSkill(Game game, string stringId, string name, string description)
        {
            var skill = game.ObjectManager.RegisterPresumedObject(new SkillObject(stringId));
            skill.Initialize(new TextObject(name), new TextObject(description), SkillObject.SkillTypeEnum.Personal);
            return skill;
        }
    }
}
