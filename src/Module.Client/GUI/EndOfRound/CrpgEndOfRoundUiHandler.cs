using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.EndOfRound;

internal class CrpgEndOfRoundUiHandler : MissionView
{
    private CrpgEndOfRoundVm _dataSource = default!;
    private GauntletLayer _gauntletLayer = default!;
    private MissionLobbyComponent _missionLobbyComponent = default!;
    private MissionScoreboardComponent _scoreboardComponent = default!;
    private MissionMultiplayerGameModeBaseClient _mpGameModeBase = default!;
    private IRoundComponent RoundComponent => _mpGameModeBase.RoundComponent;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _missionLobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _scoreboardComponent = Mission.GetMissionBehavior<MissionScoreboardComponent>();
        _mpGameModeBase = Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        ViewOrderPriority = 23;
        _dataSource = new CrpgEndOfRoundVm(_scoreboardComponent, _missionLobbyComponent, RoundComponent);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("MultiplayerEndOfRound", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        ScreenManager.SetSuspendLayer(_gauntletLayer, true);

        RoundComponent.OnRoundStarted += RoundStarted;
        _scoreboardComponent.OnRoundPropertiesChanged += OnRoundPropertiesChanged;
        RoundComponent.OnPostRoundEnded += ShowEndOfRoundUi;
        _scoreboardComponent.OnMVPSelected += OnMVPSelected;

        _missionLobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        RoundComponent.OnRoundStarted -= RoundStarted;
        _scoreboardComponent.OnRoundPropertiesChanged -= OnRoundPropertiesChanged;
        RoundComponent.OnPostRoundEnded -= ShowEndOfRoundUi;
        _scoreboardComponent.OnMVPSelected -= OnMVPSelected;

        _missionLobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource.OnFinalize();
    }

    private void RoundStarted()
    {
        ScreenManager.SetSuspendLayer(_gauntletLayer, true);
        _gauntletLayer.InputRestrictions.ResetInputRestrictions();
        _dataSource.IsShown = false;
    }

    private void OnRoundPropertiesChanged()
    {
        if (RoundComponent.RoundCount != 0 && _missionLobbyComponent.CurrentMultiplayerState !=
            MissionLobbyComponent.MultiplayerGameState.Ending)
        {
            _dataSource.Refresh();
        }
    }

    private void ShowEndOfRoundUi()
    {
        ShowEndOfRoundUi(false);
    }

    private void ShowEndOfRoundUi(bool isForced)
    {
        if (isForced || (RoundComponent.RoundCount != 0 && _missionLobbyComponent.CurrentMultiplayerState !=
                MissionLobbyComponent.MultiplayerGameState.Ending))
        {
            ScreenManager.SetSuspendLayer(_gauntletLayer, false);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Mouse);
            _dataSource.IsShown = true;
        }
    }

    private void OnPostMatchEnded()
    {
        ScreenManager.SetSuspendLayer(_gauntletLayer, true);
        _dataSource.IsShown = false;
    }

    private void OnMVPSelected(MissionPeer mvpPeer, int mvpCount)
    {
        _dataSource.OnMVPSelected(mvpPeer);
    }
}
