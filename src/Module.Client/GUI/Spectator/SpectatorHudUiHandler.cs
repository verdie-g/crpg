using Crpg.Module.Helpers;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.HUDExtensions;

namespace Crpg.Module.GUI.Spectator;

internal class SpectatorHudUiHandler : MissionView
{
    private MissionMultiplayerSpectatorHUDVM? _dataSource;
    private GauntletLayer? _gauntletLayer;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _dataSource = new MissionMultiplayerSpectatorHUDVM(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("MultiplayerSpectatorHUD", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);

        MissionScreen.OnSpectateAgentFocusIn += OnSpectatedAgentFocusIn;
        MissionScreen.OnSpectateAgentFocusOut += OnSpectatedAgentFocusOut;
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        MissionScreen.OnSpectateAgentFocusIn -= OnSpectatedAgentFocusIn;
        MissionScreen.OnSpectateAgentFocusOut -= OnSpectatedAgentFocusOut;
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }

    private void OnSpectatedAgentFocusIn(Agent followedAgent)
    {
        ReflectionHelper.InvokeMethod(_dataSource!, "OnSpectatedAgentFocusIn", new object[] { followedAgent });
    }

    private void OnSpectatedAgentFocusOut(Agent followedPeer)
    {
        ReflectionHelper.InvokeMethod(_dataSource!, "OnSpectatedAgentFocusOut", new object[] { followedPeer });
    }
}
