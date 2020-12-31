using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod.Common.UI
{
    public class CrpgAgentHud : MissionView
    {
        private GauntletLayer? _gauntletLayer;
        private GauntletMovie? _gauntletMovie;
        private CrpgAgentHudViewModel? _dataSource;

        public override void EarlyStart()
        {
            base.EarlyStart();

            var crpgUserAccessor = Mission.GetMissionBehaviour<CrpgUserAccessor>();
            var experienceTable = Mission.GetMissionBehaviour<CrpgExperienceTable>();
            _dataSource = new CrpgAgentHudViewModel(crpgUserAccessor, experienceTable);

            // localOrder sets the order the layer are drawn. + 1 to be drawn over the agent HUD.
            _gauntletLayer = new GauntletLayer(ViewOrderPriorty + 1);
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
        }
    }
}
