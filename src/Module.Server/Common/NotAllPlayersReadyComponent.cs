using Crpg.Module.Helpers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// Players are sometimes locked out of the servers with a "Not all players are ready" message. This component
/// tries to unlock them.
/// </summary>
internal class NotAllPlayersReadyComponent : MissionLogic
{
    private MissionTimer? _timer;

    public override void OnMissionTick(float dt)
    {
        _timer ??= new MissionTimer(30);
        if (!_timer.Check(reset: true))
        {
            return;
        }

        var actualPlayers = new HashSet<PlayerId>(GameNetwork.NetworkPeers.Select(p => p.VirtualPlayer.Id));

        var customServer = DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer;

        var requestedPlayers = (List<PlayerId>)ReflectionHelper.GetField(customServer, "_requestedPlayers");
        var actualRequestedPlayers = requestedPlayers.FindAll(p => actualPlayers.Contains(p));
        ReflectionHelper.SetField(customServer, "_requestedPlayers", actualRequestedPlayers);
        if (requestedPlayers.Count != actualRequestedPlayers.Count)
        {
            Debug.Print($"Removed {requestedPlayers.Count - actualRequestedPlayers.Count} stuck players from the requested players");
        }

        var customBattlePlayers = (List<PlayerId>)ReflectionHelper.GetField(customServer, "_customBattlePlayers");
        var actualCustomBattlePlayers = customBattlePlayers.FindAll(p => actualPlayers.Contains(p));
        ReflectionHelper.SetField(customServer, "_customBattlePlayers", actualCustomBattlePlayers);
        if (customBattlePlayers.Count != actualCustomBattlePlayers.Count)
        {
            Debug.Print($"Removed {customBattlePlayers.Count - actualCustomBattlePlayers.Count} stuck players from the custom battle players");
        }
    }
}
