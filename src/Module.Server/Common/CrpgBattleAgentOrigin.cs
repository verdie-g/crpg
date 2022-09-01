using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgBattleAgentOrigin : BasicBattleAgentOrigin
{
    public CharacterSkills Skills { get; }

    public CrpgBattleAgentOrigin(BasicCharacterObject? troop, CharacterSkills skills)
        : base(troop)
    {
        Skills = skills;
    }
}
