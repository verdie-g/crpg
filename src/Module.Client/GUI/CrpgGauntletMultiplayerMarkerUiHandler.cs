using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

[OverrideView(typeof(CrpgMultiplayerMarkerUIHandler))]
internal class CrpgGauntletMultiplayerMarkerUiHandler : MissionView
{
    private readonly MissionMultiplayerGameModeBaseClient _gameModeClient;
    private GauntletLayer? _gauntletLayer;
    private CrpgMissionMarkerVm? _dataSource;

    public CrpgGauntletMultiplayerMarkerUiHandler(MissionMultiplayerGameModeBaseClient gameModeClient)
    {
        _gameModeClient = gameModeClient;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _dataSource = new CrpgMissionMarkerVm(MissionScreen.CombatCamera, _gameModeClient);
        _gauntletLayer = new GauntletLayer(1);
        _gauntletLayer.LoadMovie("MPMissionMarkers", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        MissionScreen.RemoveLayer(_gauntletLayer);
        _gauntletLayer = null;
        _dataSource?.OnFinalize();
        _dataSource = null;
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        if (_dataSource == null)
        {
            return;
        }

        // Gamekeys are found in GameKeyDefinitions - ShowIndicators is by default the alt key button.
        _dataSource.IsEnabled = Input.IsGameKeyDown((int)GameKeyDefinition.ShowIndicators);
        _dataSource.Tick(dt);
    }
}
