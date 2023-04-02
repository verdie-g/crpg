using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.TeamDeathmatch;

internal class CrpgTeamDeathmatchSpawningBehavior : CrpgSpawningBehaviorBase
{
    public CrpgTeamDeathmatchSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
    }

    public override void OnTick(float dt)
    {
        if (!IsSpawningEnabled)
        {
            return;
        }

        SpawnAgents();
        SpawnBotAgents();
    }

    public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
    {
        if (peer.Team != null)
        {
            if (peer.Team.Side == BattleSideEnum.Attacker)
            {
                return MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
            }

            if (peer.Team.Side == BattleSideEnum.Defender)
            {
                return MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue();
            }
        }

        return -1;
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.Current.CurrentState == Mission.State.Continuing;
    }
}
