using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod.DefendTheVirgin
{
    public class CrpgAgentExperienceView : MissionView
    {
        private GauntletLayer? _gauntletLayer;
        private GauntletMovie? _gauntletMovie;

        public override void EarlyStart()
        {
            base.EarlyStart();
            // localOrder sets the order the layer are drawn. + 1 to be drawn over the agent HUD.
            _gauntletLayer = new GauntletLayer(ViewOrderPriorty + 1);
            _gauntletMovie = _gauntletLayer.LoadMovie("CrpgHUD", null);
            MissionScreen.AddLayer(this._gauntletLayer);
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            _gauntletLayer!.ReleaseMovie(_gauntletMovie);
        }
    }
}
