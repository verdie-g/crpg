using Crpg.Module.Modes.Skirmish;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using MathF = TaleWorlds.Library.MathF;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.StrategusBattle;

internal class CrpgStrategusBattleServer : MissionMultiplayerGameModeBase
{
    private const float FlagCaptureRange = 6f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    private const float BattleMoraleGainOnTick = 0.00175f;
    private const float BattleMoraleGainMultiplierLastFlag = 2f;
    private const float SkirmishMoraleGainOnTick = 0.00125f;
    private const float SkirmishMoraleGainMultiplierLastFlag = 2f;

    private readonly CrpgStrategusBattleClient _StrategusBattleClient;
    private readonly bool _isSkirmish;

    /// <summary>A number between -1.0 and 1.0. Less than 0 means the defenders are winning. Greater than 0 for attackers.</summary>
    private float _morale;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[,] _agentCountsAroundFlags = new int[0, 0];

    /// <summary>True if captures points were removed and only one remains.</summary>
    private bool _wereFlagsRemoved;
    private Timer? _checkFlagRemovalTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgStrategusBattleServer(CrpgStrategusBattleClient StrategusBattleClient)
    {
        _StrategusBattleClient = StrategusBattleClient;
    }

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        RoundController.OnPreRoundEnding += OnPreRoundEnding;

        AddTeams();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _morale = 0f;
        // TODO: SetTeamColorsWithAllSynched
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnClearScene()
    {
        _morale = 0.0f;
        _checkFlagRemovalTimer = null;
        _wereFlagsRemoved = false;
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

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !RoundController.IsRoundInProgress
            || !CanGameModeSystemsTickThisFrame
            || _flags.Length == 0) // Protection against scene with no flags.
        {
            return;
        }
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        bool defenderTeamDepleted = false; // todo
        bool attackerTeamDepleted = false; // todo
        return defenderTeamDepleted || attackerTeamDepleted;

    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    public override bool CheckIfOvertime()
    {
        return false;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.EndModuleEventAsServer();
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<TeamDeathmatchMissionRepresentative>();
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

    /// <summary>Checks the flag index are from 0 to N.</summary>
    private void ThrowOnBadFlagIndexes(FlagCapturePoint[] flags)
    {
        int expectedIndex = 0;
        foreach (var flag in flags.OrderBy(f => f.FlagIndex))
        {
            if (flag.FlagIndex != expectedIndex)
            {
                throw new Exception($"Invalid scene '{Mission.Current?.SceneName}': Flag indexes should be numbered from 0 to {flags.Length}");
            }

            expectedIndex += 1;
        }
    }

    private void OnPreRoundEnding()
    {
        bool defenderTeamAliveDepleted = false; // check how many lives the team has
        bool attackerTeamAliveDepleted = false; // check how many lives the team has
        CaptureTheFlagCaptureResultEnum roundResult;
        if (!defenderTeamAliveDepleted)
        {
            roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
            RoundController.RoundWinner = BattleSideEnum.Defender;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }
        else if (!attackerTeamAliveDepleted)
        {
            roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
            RoundController.RoundWinner = BattleSideEnum.Attacker;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }
        else // Everyone ded
        {
            roundResult = CaptureTheFlagCaptureResultEnum.Draw;
            RoundController.RoundWinner = BattleSideEnum.None;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }

        Debug.Print($"Team {RoundController.RoundWinner} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");
        CheerForRoundEnd(roundResult);
    }

    private void CheerForRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
    {
        AgentVictoryLogic missionBehavior = Mission.GetMissionBehavior<AgentVictoryLogic>();
        if (roundResult == CaptureTheFlagCaptureResultEnum.AttackersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Attacker);
        }
        else if (roundResult == CaptureTheFlagCaptureResultEnum.DefendersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Defender);
        }
    }
}
