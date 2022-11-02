using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

[OverrideView(typeof(CrpgMultiplayerMarkerUIHandler))]
internal class CrpgGauntletMultiplayerMarkerUIHandler : MissionView
{
    private readonly MissionMultiplayerGameModeBaseClient _gameModeClient;
    private GauntletLayer? _gauntletLayer;
    private CrpgMissionMarkerVM? _dataSource;

    public CrpgGauntletMultiplayerMarkerUIHandler(MissionMultiplayerGameModeBaseClient gameModeClient)
    {
        _gameModeClient = gameModeClient;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _dataSource = new CrpgMissionMarkerVM(MissionScreen.CombatCamera, _gameModeClient);
        _gauntletLayer = new GauntletLayer(1, "GauntletLayer", false);
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

        _dataSource.IsEnabled = Input.IsGameKeyDown(5);
        _dataSource.Tick(dt);
    }
}
