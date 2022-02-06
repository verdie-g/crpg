using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Crpg.GameMod.Common;

public class CrpgSkills
{
    public static SkillObject Strength { get; private set; } = default!;
    public static SkillObject Agility { get; private set; } = default!;
    public static SkillObject IronFlesh { get; private set; } = default!;
    public static SkillObject PowerStrike { get; private set; } = default!;
    public static SkillObject PowerDraw { get; private set; } = default!;
    public static SkillObject PowerThrow { get; private set; } = default!;
    public static SkillObject WeaponMaster { get; private set; } = default!;
    public static SkillObject MountedArchery { get; private set; } = default!;
    public static SkillObject Shield { get; private set; } = default!;

    public static void Initialize(Game game)
    {
        Strength = InitializeSkill(game, nameof(Strength), "Strength", string.Empty);
        Agility = InitializeSkill(game, nameof(Agility), "Agility", string.Empty);
        IronFlesh = InitializeSkill(game, nameof(IronFlesh), "Iron Flesh", string.Empty);
        PowerStrike = InitializeSkill(game, nameof(PowerStrike), "Power Strike", string.Empty);
        PowerDraw = InitializeSkill(game, nameof(PowerDraw), "Power Draw", string.Empty);
        PowerThrow = InitializeSkill(game, nameof(PowerThrow), "Power Throw", string.Empty);
        WeaponMaster = InitializeSkill(game, nameof(WeaponMaster), "Weapon Master", string.Empty);
        MountedArchery = InitializeSkill(game, nameof(MountedArchery), "Mounted Archery", string.Empty);
        Shield = InitializeSkill(game, nameof(Shield), "Shield", string.Empty);
    }

    private static SkillObject InitializeSkill(Game game, string stringId, string name, string description)
    {
        var skill = game.ObjectManager.RegisterPresumedObject(new SkillObject(stringId));
        skill.Initialize(new TextObject(name), new TextObject(description), SkillObject.SkillTypeEnum.Personal);
        return skill;
    }
}
