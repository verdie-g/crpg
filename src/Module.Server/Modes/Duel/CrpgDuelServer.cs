using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Duel;

/// <summary>
/// The cRPG Duel implementation.
/// Unfortunately we cannot just copy the entire MissionMultiplayerDuel class.
/// A lot of duel GUI elements check for the MissionMultiplayerDuel mission. Without a declared MissionMultiplayerDuel
/// the game will just crash.
/// </summary>
internal class CrpgDuelServer : MissionMultiplayerDuel
{
    private readonly CrpgRewardServer _rewardServer;
    private MissionTimer? _rewardTickTimer;

    public CrpgDuelServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
    }

    // If the GameType is duel is crashes after each duel. I was not able to debug it.
    // Around line 432 in MissionCustomGameServerComponent - OnDuelEnded null exception for GetCurrentBattleResult or something like that
    public override MultiplayerGameType GetMissionType() => MultiplayerGameType.FreeForAll;

    public override void OnAgentRemoved(Agent? affectedAgent, Agent? affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (affectorAgent == null
            || affectedAgent == null
            || !affectedAgent.IsHuman
            || affectedAgent == affectorAgent)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            return;
        }

        // Defender Team = Players in duels
        if (affectedAgent.MissionPeer?.Team.IsDefender ?? false)
        {
            // Set the respawn timer for both players to 5.1sec. 2sec delay + 3 seconds countdown.
            float respawnDelay = 5.1f; // Has to be bigger than 2.1sec. 2seconds delay and 100ms delay to despawn the agent.
            affectedAgent.MissionPeer.SpawnTimer.Reset(Mission.CurrentTime, respawnDelay); // was 5.1f
            affectorAgent.MissionPeer.SpawnTimer.Reset(Mission.CurrentTime, respawnDelay);
            _ = RemoveRemainingAgents(affectorAgent.MissionPeer, respawnDelay - 2.5f); // Should not be lower than 2.5f, otherwise the duel score will not increase!
        }

        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        RewardUsers();
    }

    protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        base.HandleLateNewClientAfterSynchronized(networkPeer);
        // Remove player from list to reset preferred arena type.
        if (SpawnComponent?.SpawningBehavior is CrpgDuelSpawningBehavior duelSpawningBehavior)
        {
            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
            duelSpawningBehavior.UpdatedPlayerPreferredArenaOnce.Remove(networkPeer.VirtualPlayer.Id);
            missionPeer?.SpawnTimer?.AdjustStartTime(-3f); // Used to reduce the initial spawn on connect.
        }
    }

    private void RewardUsers()
    {
        _rewardTickTimer ??= new MissionTimer(duration: 60);
        if (_rewardTickTimer.Check(reset: true))
        {
            _ = _rewardServer.UpdateCrpgUsersAsync(durationRewarded: 0, updateUserStats: false);
        }
    }

    private async Task RemoveRemainingAgents(MissionPeer? missionPeer, float delay)
    {
        await Task.Delay((int)(delay * 1000) - 100); // After 2 seconds the duel is actually over. So we wait a bit longer before we manually remove the player who is alive.
        missionPeer?.ControlledAgent?.FadeOut(true, true);
    }
}
