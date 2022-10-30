using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Duel;

internal class CrpgDuelMissionMultiplayer : MissionMultiplayerDuel
{
    // If the GameType is duel is crashes after each duel. I was not able to debug it.
    // Around line 432 in MissionCustomGameServerComponent - OnDuelEnded null exception for GetCurrentBattleResult or something like that
    public override MissionLobbyComponent.MultiplayerGameType GetMissionType() => MissionLobbyComponent.MultiplayerGameType.FreeForAll;

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (!affectedAgent.IsHuman || affectorAgent == null || affectedAgent == null || affectedAgent == affectorAgent)
        {
            return;
        }

        // Defender Team = Players in duels
        if (affectedAgent.MissionPeer.Team.IsDefender)
        {
            // Set the respawn timer for both players to 5.1sec. 2sec delay + 3 seconds countdown.
            float respawnDelay = 2.5f; // Has to be bigger than 2.1sec. 2seconds delay and 100ms delay to despawn the agent.
            affectedAgent.MissionPeer.SpawnTimer.Reset(Mission.CurrentTime, respawnDelay); // was 5.1f
            affectorAgent.MissionPeer.SpawnTimer.Reset(Mission.CurrentTime, respawnDelay);
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            _ = RemoveRemainingAgents(affectorAgent.MissionPeer, respawnDelay);
        }
        else
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        }
    }

    private async Task RemoveRemainingAgents(MissionPeer peer, float delay)
    {
        await Task.Delay(Convert.ToInt32(delay * 1000) - 100); // After 2 seconds the duel is actually over. So we wait a bit longer before we manually remove the player who is alive.
        if (peer == null)
        {
            return;
        }

        Agent controlledAgent = peer.ControlledAgent;
        if (controlledAgent != null)
        {
            controlledAgent.FadeOut(true, true);
        }
    }
}
