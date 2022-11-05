using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Duel;

/// <summary>
/// The cRPG Duel implementation.
/// Unfortunately we cannot just copy the entire MissionMultiplayerDuel class.
/// A lot of duel GUI elements check for the MissionMultiplayerDuel mission. Without a declared MissionMultiplayerDuel
/// the game will just crash.
/// </summary>
internal class CrpgDuelMissionMultiplayer : MissionMultiplayerDuel
{
    private readonly CrpgHttpClient _crpgClient;
    private float _checkUserUpdate;

    public CrpgDuelMissionMultiplayer(CrpgHttpClient crpgClient)
    {
        _crpgClient = crpgClient;
        _checkUserUpdate = 0;
    }

    // If the GameType is duel is crashes after each duel. I was not able to debug it.
    // Around line 432 in MissionCustomGameServerComponent - OnDuelEnded null exception for GetCurrentBattleResult or something like that
    public override MissionLobbyComponent.MultiplayerGameType GetMissionType() => MissionLobbyComponent.MultiplayerGameType.FreeForAll;

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
        if (Mission.Current.CurrentTime - _checkUserUpdate >= 30f) // Update all character data all 30sec
        {
            _checkUserUpdate = Mission.Current.CurrentTime;
            UpdateCrpgIdleAgentCharacters();
        }
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

    private async Task RemoveRemainingAgents(MissionPeer missionPeer, float delay)
    {
        await Task.Delay((int)(delay * 1000) - 100); // After 2 seconds the duel is actually over. So we wait a bit longer before we manually remove the player who is alive.
        missionPeer?.ControlledAgent?.FadeOut(true, true);
    }

    /// <summary>
    /// Update all players which are not in a duel and not spectating.
    /// </summary>
    private void UpdateCrpgIdleAgentCharacters()
    {
        List<NetworkCommunicator> playersToUpdate = new();
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer? missionPeer = networkPeer?.GetComponent<MissionPeer>();
            CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (networkPeer == null || missionPeer == null ||
                crpgPeer == null || crpgPeer.User == null ||
                missionPeer.Team != Mission.AttackerTeam) // AttackerTeam = Players which are not spectator and not live in a duel.
            {
                continue;
            }

            playersToUpdate.Add(networkPeer);
        }

        _ = UpdateCrpgDuelistUsersAsync(playersToUpdate.ToArray());
    }

    private async Task UpdateCrpgDuelistUsersAsync(NetworkCommunicator[] networkPeers)
    {
        List<CrpgUserUpdate> userUpdates = new();
        Dictionary<int, CrpgPeer> crpgPeerByUserId = new();

        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer?.User == null)
            {
                continue;
            }

            crpgPeerByUserId[crpgPeer.User.Id] = crpgPeer;
            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgPeer.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics { Kills = 0, Deaths = 0, Assists = 0, PlayTime = TimeSpan.Zero },
                Rating = crpgPeer.User!.Character.Rating,
                BrokenItems = Array.Empty<CrpgUserBrokenItem>(),
            };

            userUpdates.Add(userUpdate);
        }

        if (userUpdates.Count == 0)
        {
            return;
        }

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        try
        {
            var res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
            ApplyUpdatedPlayerData(res.UpdateResults, crpgPeerByUserId);
            foreach (NetworkCommunicator networkPeer in networkPeers)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotification.NotificationType.Notification,
                    Message = $"Your character was updated.",
                    IsMessageTextId = false,
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
        }
    }

    private void ApplyUpdatedPlayerData(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgPeer> crpgPeerByUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgPeerByUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgPeer.User = updateResult.User;
        }
    }
}
