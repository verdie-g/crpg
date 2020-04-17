using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod
{
    
    [MissionManager]
    public class CrpgBattleMissionBasedMultiplayerGamemode : MissionBasedMultiplayerGameMode
    {
        public CrpgBattleMissionBasedMultiplayerGamemode(string name) : base(name)
        {
        }

        public override void JoinCustomGame(JoinGameData joinGameData)
        {
            LobbyGameStateCustomGameClient lobbyGameStateCustomGameClient = Game.Current.GameStateManager.CreateState<LobbyGameStateCustomGameClient>();
            lobbyGameStateCustomGameClient.SetStartingParameters(NetworkMain.GameClient, joinGameData.GameServerProperties.Address, joinGameData.GameServerProperties.Port, joinGameData.GameServerProperties.GameType, joinGameData.GameServerProperties.Map, joinGameData.PeerIndex, joinGameData.SessionKey);
            Game.Current.GameStateManager.PushState(lobbyGameStateCustomGameClient, 0);
        }
        public override void StartMultiplayerGame(string scene)
        {
            if (base.Name == "CrpgBattle")
            {

	            ScoreboardFactory.Register("CrpgBattle", new CrpgBattleModeScoreData());
                OpenCrpgBattleMission(scene);
                return;
            }
         
        }

        [MissionMethod]
        public static void OpenCrpgBattleMission(string scene)
        {
            MissionState.OpenNew("CrpgBattle", new MissionInitializerRecord(scene), delegate (Mission missionController)
            {
                if (GameNetwork.IsServer)
                {
                    return new MissionBehaviour[]
                    {
                            (MissionBehaviour)MissionLobbyComponent.CreateBehaviour(),
                            new MissionMultiplayerCrpgBattle(),
                            new MissionMultiplayerCrpgBattleClient(),
                            new MultiplayerTimerComponent(),
                            new MultiplayerMissionAgentVisualSpawnComponent(),
                            new SpawnComponent((SpawnFrameBehaviourBase)new TeamDeathmatchSpawnFrameBehavior(), new CrpgBattleSpawningBehavior()),
                            new MissionLobbyEquipmentNetworkComponent(),
                            new MultiplayerTeamSelectComponent(),
                            new MissionHardBorderPlacer(),
                            new MissionBoundaryPlacer(),
                            new MissionBoundaryCrossingHandler(),
                            new MultiplayerPollComponent(),
                            new MultiplayerAdminComponent(),
                            new MultiplayerGameNotificationsComponent(),
                            new MissionOptionsComponent(),
                            new MissionScoreboardComponent("CrpgBattle"),
                            new AgentBattleAILogic(),
                            new AgentFadeOutLogic()
                    };
                }
                return new MissionBehaviour[]
                {
                    MissionLobbyComponent.CreateBehaviour(),
                    new MissionMultiplayerTeamDeathmatchClient(),
                    new MultiplayerTimerComponent(),
                    new MultiplayerMissionAgentVisualSpawnComponent(),
                    new MissionLobbyEquipmentNetworkComponent(),
                    new MultiplayerTeamSelectComponent(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new MultiplayerPollComponent(),
                    new MultiplayerGameNotificationsComponent(),
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent("CrpgBattle")
                };
            }, true, true, false);
        }
    }
}

class CrpgBattleMissionOptionsComponent : MissionOptionsComponent
{
    public new void OnAddOptionsUIHandler()
    {
        Debug.DebugManager.Print("CrpgBattleMissionOptionsComponent::OnAddOptionsUIHandler");
        base.OnAddOptionsUIHandler();
    }
}

[ViewCreatorModule]
public class MultiplayerMissionViews
{

	[ViewMethod("CrpgBattle")]
	public static MissionView[] OpenTeamDeathmatchMission(Mission mission)
	{
	    List<MissionView> list = new List<MissionView>();
	    list.Add(ViewCreator.CreateLobbyUIHandler());
	    list.Add(ViewCreator.CreateMissionMultiplayerPreloadView(mission));
	    list.Add(ViewCreator.CreateMultiplayerTeamSelectUIHandler());
	    list.Add(ViewCreator.CreateMissionKillNotificationUIHandler());
	    list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
	    list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
	    list.Add(ViewCreator.CreateMissionMultiplayerEscapeMenu("CrpgBattle"));
	    list.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
	    list.Add(ViewCreator.CreateMultiplayerEndOfRoundUIHandler());
	    list.Add(ViewCreator.CreateLobbyEquipmentUIHandler());
	    list.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
	    list.Add(ViewCreator.CreatePollInitiationUIHandler());
	    list.Add(ViewCreator.CreatePollProgressUIHandler());
	    list.Add(ViewCreator.CreateMissionFlagMarkerUIHandler());
	    list.Add(ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler());
	    list.Add(ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(null));
	    list.Add(ViewCreator.CreateOptionsUIHandler());
	    if (!GameNetwork.IsClient)
	    {
		    list.Add(ViewCreator.CreateMultiplayerAdminPanelUIHandler());
	    }
	    list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
	    list.Add(new MissionBoundaryWallView());
	    list.Add(new MissionItemContourControllerView());
	    list.Add(new MissionAgentContourControllerView());
	    return list.ToArray();
	}
}