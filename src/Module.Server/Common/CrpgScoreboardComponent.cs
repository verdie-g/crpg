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

        Agent affectorCharacterAgent = affectorAgent.IsMount ? affectorAgent.RiderAgent : affectorAgent;
        Agent affectedCharacterAgent = affectedAgent.IsMount ? affectedAgent.RiderAgent : affectedAgent;
        if (affectedCharacterAgent == null)
        {
            return;
        }

        float ratingFactor;
        var affectedCrpgUser = affectedCharacterAgent.MissionPeer?.GetComponent<CrpgPeer>()?.User;
        if (affectedCrpgUser == null)
        {
            // The affected agent is probably a bot.
            ratingFactor = 0.8f;
        }
        else
        {
            double ratingWithUncertainty = 0.01 * (affectedCrpgUser.Character.Rating.Value - 2 * affectedCrpgUser.Character.Rating.Deviation);
            float competitiveRating = (float)(ratingWithUncertainty < 0
                ? 0.03f * -Math.Pow(-ratingWithUncertainty, 3.98)
                : 0.03f * Math.Pow(ratingWithUncertainty, 3.98));
            const float ratingToScoreScaler = 1 / 1250f;
            ratingFactor = competitiveRating * ratingToScoreScaler * CrpgRatingHelper.ComputeRegionRatingPenalty(affectedCrpgUser.Region);
        }

        float score = damagedHp / affectedCharacterAgent.BaseHealthLimit * 100f * ratingFactor;
        if (isBlocked)
        {
            if (!collisionData.AttackBlockedWithShield)
            {
                return;
            }

            score = (collisionData.InflictedDamage + collisionData.AbsorbedByArmor) / 12.5f;
        }
        else if (affectedAgent.IsMount && affectedCharacterAgent != null)
        {
            score = damagedHp / affectedAgent.BaseHealthLimit * 70f * ratingFactor;
        }

        if (affectorCharacterAgent == affectedCharacterAgent)
        {
            return;
        }

        var affectorMissionPeer = affectorCharacterAgent?.MissionPeer;
        if (affectorMissionPeer != null)
        {
            score = affectorCharacterAgent!.IsFriendOf(affectedCharacterAgent) ? score * -1f : score;
            ReflectionHelper.SetProperty(
                affectorMissionPeer,
                nameof(affectorMissionPeer.Score),
                (int)(affectorMissionPeer.Score + score));

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new KillDeathCountChange(affectorMissionPeer.GetNetworkPeer(),
                null, affectorMissionPeer.KillCount, affectorMissionPeer.AssistCount, affectorMissionPeer.DeathCount,
                affectorMissionPeer.Score));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
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
