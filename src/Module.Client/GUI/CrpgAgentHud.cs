using Crpg.Module.Common;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.Module.GUI;

internal class CrpgAgentHud : MissionView
{
    private readonly CrpgExperienceTable _experienceTable;
    private GauntletLayer? _gauntletLayer;
    private IGauntletMovie? _gauntletMovie;
    private CrpgAgentHudViewModel? _dataSource;

    public CrpgAgentHud(CrpgExperienceTable experienceTable)
    {
        _experienceTable = experienceTable;
    }

    public override void EarlyStart()
    {
        base.EarlyStart();

        _dataSource = new CrpgAgentHudViewModel(_experienceTable);

        // localOrder sets the order the layer are drawn. + 1 to be drawn over the agent HUD.
        _gauntletLayer = new GauntletLayer(ViewOrderPriority + 1);
        _gauntletMovie = _gauntletLayer.LoadMovie("CrpgAgentHud", _dataSource); // Load the file from GUI/Prefabs.
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenTick(float dt)
    {
        _dataSource!.Tick(dt);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        _gauntletLayer!.ReleaseMovie(_gauntletMovie);
        MissionScreen.RemoveLayer(_gauntletLayer);
    }
}
