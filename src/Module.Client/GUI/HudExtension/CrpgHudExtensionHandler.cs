using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.HudExtension;

/// <summary>
/// Copy of TaleWorlds.MountAndBlade.GauntletUI.Mission.Multiplayer.MissionGauntletMultiplayerHUDExtension. Responsible
/// for updating the HUD at the top of the screen (flag, morale, ...).
/// </summary>
internal class CrpgHudExtensionHandler : MissionView
{
    private CrpgHudExtensionVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private SpriteCategory? _mpMissionCategory;
    private MissionLobbyComponent? _lobbyComponent;

    public CrpgHudExtensionHandler()
    {
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _mpMissionCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

        _dataSource = new CrpgHudExtensionVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("HUDExtension", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        MissionScreen.OnSpectateAgentFocusIn += _dataSource.OnSpectatedAgentFocusIn;
        MissionScreen.OnSpectateAgentFocusOut += _dataSource.OnSpectatedAgentFocusOut;
        Game.Current.EventManager.RegisterEvent<MissionPlayerToggledOrderViewEvent>(OnMissionPlayerToggledOrderViewEvent);
        _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
    }

    public override void OnMissionScreenFinalize()
    {
        _lobbyComponent!.OnPostMatchEnded -= OnPostMatchEnded;
        MissionScreen.OnSpectateAgentFocusIn -= _dataSource!.OnSpectatedAgentFocusIn;
        MissionScreen.OnSpectateAgentFocusOut -= _dataSource.OnSpectatedAgentFocusOut;
        MissionScreen.RemoveLayer(_gauntletLayer);
        _mpMissionCategory?.Unload();
        _dataSource.OnFinalize();
        _dataSource = null;
        _gauntletLayer = null;
        Game.Current.EventManager.UnregisterEvent<MissionPlayerToggledOrderViewEvent>(OnMissionPlayerToggledOrderViewEvent);
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }

    private void OnMissionPlayerToggledOrderViewEvent(MissionPlayerToggledOrderViewEvent eventObj)
    {
        _dataSource!.IsOrderActive = eventObj.IsOrderEnabled;
    }

    private void OnPostMatchEnded()
    {
        _dataSource!.ShowHud = false;
    }
}
