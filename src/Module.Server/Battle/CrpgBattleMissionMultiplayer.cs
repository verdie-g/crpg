using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Battle;

internal class CrpgBattleMissionMultiplayer : MissionMultiplayerGameModeBase
{
    private CrpgBattleMissionMultiplayerClient? _client;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _client = Mission.Current.GetMissionBehavior<CrpgBattleMissionMultiplayerClient>();
    }

    public override void AfterStart()
    {
        base.AfterStart();
        RoundController.OnPreRoundEnding += OnRoundEnd;
        RoundController.OnPostRoundEnded += OnPostRoundEnd;

        AddTeams();
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnRoundEnd;
        RoundController.OnPostRoundEnded -= OnPostRoundEnd;
        base.OnRemoveBehavior();
    }

    public override void OnPeerChangedTeam(NetworkCommunicator peer, Team? oldTeam, Team newTeam)
    {
        // TODO: reward multiplier to change maybe
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            return;
        }
    }

    public override bool CheckForWarmupEnd()
    {
        int playersInTeam = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
            {
                playersInTeam += 1;
            }
        }

        return playersInTeam >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
    }

    public override bool CheckForRoundEnd()
    {
        // TODO: round time?
        return Mission.AttackerTeam.ActiveAgents.Count == 0
               || Mission.DefenderTeam.ActiveAgents.Count == 0;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    private void AddTeams()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, bannerTeam2, false, true);
    }

    private void OnRoundEnd()
    {
        RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        // TODO:
    }

    private void OnPostRoundEnd()
    {
    }
}
