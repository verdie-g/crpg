using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Conquest;

internal class ConquestHudUiHandler : MissionView
{
    private ConquestHudVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private SpriteCategory? _mpMissionCategory;

    public ConquestHudUiHandler()
    {
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _mpMissionCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

        _dataSource = new ConquestHudVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("ConquestHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        _mpMissionCategory?.Unload();
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }
}
