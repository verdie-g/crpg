using TaleWorlds.MountAndBlade;
using Crpg.Module.Api.Models.Users;

#if CRPG_SERVER
using System.Text;
using Crpg.Module.Balancing;
using Crpg.Module.Common;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.PlayerServices;
#endif

namespace Crpg.Module.Modes.Dtv;

/// <summary>
/// Disables team selection and randomly assign teams if the native balancer is enabled. Else use the cRPG balancer
/// to balance teams after each round.
/// </summary>
internal class CrpgDtvTeamSelectComponent : MultiplayerTeamSelectComponent
{
#if CRPG_SERVER
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly MultiplayerRoundController? _roundController;
    private readonly PeriodStatsHelper _periodStatsHelper;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgDtvTeamSelectComponent(MultiplayerWarmupComponent warmupComponent, MultiplayerRoundController? roundController)
    {
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _periodStatsHelper = new PeriodStatsHelper();
        _playersWaitingForTeam = new HashSet<PlayerId>();
    }
#endif

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

#if CRPG_SERVER
        _warmupComponent.OnWarmupEnded += OnWarmupEnded;
        if (_roundController != null)
        {
            _roundController.OnPostRoundEnded += OnRoundEnded;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnPostRoundEnded -= OnRoundEnded;
        }

        _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<TeamChange>(HandleTeamChange);
    }

    private bool HandleTeamChange(NetworkCommunicator peer, TeamChange message)
    {
        if (message.Team != Mission.SpectatorTeam | message.AutoAssign)
        {
            ChangeTeamServer(peer, Mission.DefenderTeam);
        }
        else
        {
            ChangeTeamServer(peer, message.Team);
        }

        var missionPeer = peer.GetComponent<MissionPeer>();
        if (missionPeer is { Team: null })
        {
            // If the player just connected to the server, assign their team so they have a chance
            // to play the round.
            ChangeTeamServer(peer, Mission.DefenderTeam);
        }
        else
        {
            _playersWaitingForTeam.Add(peer.VirtualPlayer.Id);
        }

        return true;
    }

    private void OnRoundEnded()
    {

    }

    private void OnWarmupEnded()
    {

    }

#else
    }
#endif
}
