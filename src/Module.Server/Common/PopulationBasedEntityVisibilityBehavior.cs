using Crpg.Module.Notifications;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Shows/hides entities depending on the population size.
/// </summary>
internal class PopulationBasedEntityVisibilityBehavior : MissionLogic
{
    private readonly MissionLobbyComponent _lobbyComponent;
    private bool _gameStarted;

    public PopulationBasedEntityVisibilityBehavior(MissionLobbyComponent lobbyComponent)
    {
        _lobbyComponent = lobbyComponent;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_lobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            return;
        }

        if (_gameStarted)
        {
            return;
        }

        _gameStarted = true;
        string populationSize = ResolvePopulationSize();
        string tag = $"crpg_population_{populationSize}_hidden";
        int removedCount = 0;
        foreach (var entity in Mission.Scene.FindEntitiesWithTag(tag).ToArray())
        {
            var synchedMissionObject = entity.GetScriptComponents<SynchedMissionObject>().FirstOrDefault();
            if (synchedMissionObject == null)
            {
                Debug.Print($"Entity has a {tag} but no {nameof(SynchedMissionObject)} script");
                continue;
            }

            synchedMissionObject.SetVisibleSynched(false);
            removedCount += 1;
        }

        if (removedCount == 0)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgNotificationId
        {
            Type = CrpgNotificationType.Notification,
            TextId = "str_notification",
            TextVariation = "entities_removed",
            SoundEvent = string.Empty,
            Variables = { ["ENTITY_COUNT"] = removedCount.ToString() },
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private string ResolvePopulationSize()
    {
        int playerCount = CountPlayers();
        return playerCount switch
        {
            > 70 => "large",
            > 35 => "medium",
            _ => "small",
        };
    }

    private int CountPlayers()
    {
        int count = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer?.Team == null
                || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            count += 1;
        }

        return count;
    }
}
