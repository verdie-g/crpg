using Crpg.GameMod.Common.UI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod.DefendTheVirgin
{
    [ViewCreatorModule]
    public class DefendTheVirginViews
    {
        [ViewMethod("DefendTheVirgin")]
        public static MissionView[] OpenDefendTheVirginMission(Mission mission) => new[]
        {
            ViewCreator.CreateMissionSingleplayerEscapeMenu(),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreatorManager.CreateMissionView<CrpgAgentHud>(false, mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission),
            ViewCreator.CreateMissionMainAgentCheerControllerView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
            ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
            new MissionAgentContourControllerView(),
        };
    }
}
