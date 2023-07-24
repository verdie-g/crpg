using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Warmup;

internal class WarmupHudUiHandler : MissionView
{
    private WarmupHudVm? _dataSource;
    private GauntletLayer? _gauntletLayer;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _dataSource = new WarmupHudVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("WarmupHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }
}
