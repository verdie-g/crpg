using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using Crpg.Module.Rating;
#endif

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
#if CRPG_SERVER
        if (damagedHp < 0 || collisionData.InflictedDamage < 0) // Happens when the damage is disabled on round end.
        {
            return;
        }

        affectorAgent = affectorAgent.IsMount ? affectorAgent.RiderAgent : affectorAgent;
        affectedAgent = affectedAgent.IsMount ? affectedAgent.RiderAgent : affectedAgent;

        var affectorCrpgUser = affectorAgent?.MissionPeer?.GetComponent<CrpgPeer>()?.User;
        var affectedCrpgUser = affectedAgent?.MissionPeer?.GetComponent<CrpgPeer>()?.User;
        if (affectorCrpgUser == null || affectedCrpgUser == null)
        {
            return;
        }

        const float ratingToScoreScaler = 0.001f;
        float rating = affectedCrpgUser.Character.Rating.Value - 2 * affectedCrpgUser.Character.Rating.Deviation;
        float ratingFactor = rating * ratingToScoreScaler * CrpgRatingHelper.ComputeRegionRatingPenalty(affectedCrpgUser.Region);

        float score = damagedHp * ratingFactor;
        if (isBlocked)
        {
            if (!collisionData.AttackBlockedWithShield)
            {
                return;
            }

            score = collisionData.InflictedDamage * 0.3f;
        }
        else if (affectedAgent!.IsMount)
        {
            score = damagedHp * 0.45f;
        }

        if (affectorAgent == affectedAgent)
        {
            return;
        }

        var affectorMissionPeer = affectorAgent!.MissionPeer!;
        score = affectorAgent.IsFriendOf(affectedAgent) ? score * -1.5f : score;
        ReflectionHelper.SetProperty(affectorMissionPeer, nameof(affectorMissionPeer.Score), (int)(affectorMissionPeer.Score + score));

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new KillDeathCountChange(affectorMissionPeer.GetNetworkPeer(),
            null, affectorMissionPeer.KillCount, affectorMissionPeer.AssistCount, affectorMissionPeer.DeathCount, affectorMissionPeer.Score));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
#endif
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            // Not the best place to register to that event but heh.
            registerer.Register<UpdateRoundSpawnCount>(OnUpdateRoundSpawnCount);
        }
    }

    private void OnUpdateRoundSpawnCount(UpdateRoundSpawnCount message)
    {
        var missionPeer = message.Peer.GetComponent<MissionPeer>();
        if (missionPeer != null)
        {
            missionPeer.SpawnCountThisRound = message.SpawnCount;
        }
    }
}
