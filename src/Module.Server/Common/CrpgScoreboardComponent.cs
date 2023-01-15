using Crpg.Module.Helpers;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgScoreboardComponent : MissionScoreboardComponent
{
    public CrpgScoreboardComponent(IScoreboardData scoreboardData)
        : base(scoreboardData)
    {
    }

    public override void OnScoreHit(
        Agent affectedAgent,
        Agent affectorAgent,
        WeaponComponentData attackerWeapon,
        bool isBlocked,
        bool isSiegeEngineHit,
        in Blow blow,
        in AttackCollisionData collisionData,
        float damagedHp,
        float hitDistance,
        float shotDifficulty)
    {
        if (!GameNetwork.IsServer || isBlocked || damagedHp <= 0.0)
        {
            return;
        }

        if (affectorAgent.IsMount)
        {
            affectorAgent = affectorAgent.RiderAgent;
        }

        if (affectorAgent == null)
        {
            return;
        }

        var missionPeer = affectorAgent.MissionPeer ??
                          (!affectorAgent.IsAIControlled || affectorAgent.OwningAgentMissionPeer == null
                              ? null
                              : affectorAgent.OwningAgentMissionPeer);
        if (missionPeer == null)
        {
            return;
        }

        float score = damagedHp;
        if (affectedAgent.IsMount)
        {
            score = damagedHp * 0.45f;
            affectedAgent = affectedAgent.RiderAgent;
        }
        else if (collisionData.AttackBlockedWithShield)
        {
            score = collisionData.InflictedDamage * 0.2f;
        }

        if (affectedAgent == null || affectorAgent == affectedAgent)
        {
            return;
        }

        score = affectorAgent.IsFriendOf(affectedAgent) ? score * -1.5f : score;
        ReflectionHelper.SetProperty(missionPeer, nameof(missionPeer.Score), (int)(missionPeer.Score + score));

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new KillDeathCountChange(missionPeer.GetNetworkPeer(),
            null, missionPeer.KillCount, missionPeer.AssistCount, missionPeer.DeathCount, missionPeer.Score));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}
