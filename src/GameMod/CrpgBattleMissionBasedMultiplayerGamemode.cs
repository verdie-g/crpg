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
            if (base.Name == "ClassicBattle")
            {
                MultiplayerOptions.MultiplayerOptionsAccessMode mode = MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions;
                MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(12, mode);
                MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
                MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(100, mode);
                MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(100, mode);
                MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(100, mode);
                MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(100, mode);
                MultiplayerOptions.OptionType.SpectatorCamera.SetValue(6, mode);
                MultiplayerOptions.OptionType.WarmupTimeLimit.SetValue(0, mode);
                MultiplayerOptions.OptionType.MapTimeLimit.SetValue(5, mode);
                MultiplayerOptions.OptionType.RoundTimeLimit.SetValue(240, mode);
                MultiplayerOptions.OptionType.RoundPreparationTimeLimit.SetValue(10, mode);
                MultiplayerOptions.OptionType.RoundTotal.SetValue(5, mode);
                //MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
                //MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
                //MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
                //MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
                MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(0, mode);
                MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.SetValue(0, mode);
                MultiplayerOptions.OptionType.UseRealisticBlocking.SetValue(1, mode);
                MultiplayerOptions.OptionType.NumberOfBotsTeam1.SetValue(30, mode);
                MultiplayerOptions.OptionType.NumberOfBotsTeam2.SetValue(30, mode);

                ScoreboardFactory.Register("ClassicBattle", new CrpgBattleModeScoreData());
                OpenCrpgBattleMission(scene);
                return;
            }
         
        }

        [MissionMethod]
        public static void OpenCrpgBattleMission(string scene)
        {
            MissionState.OpenNew("ClassicBattle", new MissionInitializerRecord(scene), delegate (Mission missionController)
            {
                if (GameNetwork.IsServer)
                {
                    InformationManager.DisplayMessage(new InformationMessage("OpenCrpgBattleMission::IsServer"));
                    return new MissionBehaviour[]
                    {
                            (MissionBehaviour)MissionLobbyComponent.CreateBehaviour(),
                            new MultiplayerRoundController(), //+
                            new MissionMultiplayerCrpgBattle(),
                            //new MultiplayerWarmupComponent(), //+
                            new MissionMultiplayerCrpgBattleClient(),
                            new MultiplayerTimerComponent(),
                            new MultiplayerMissionAgentVisualSpawnComponent(),
                            //new SpawnComponent((SpawnFrameBehaviourBase)new TeamDeathmatchSpawnFrameBehavior(), new CrpgBattleSpawningBehavior()),
                            new SpawnComponent((SpawnFrameBehaviourBase)new FlagDominationSpawnFrameBehaviour(), new CrpgBattleSpawningBehavior()),
                            new MissionLobbyEquipmentNetworkComponent(),
                            new MultiplayerTeamSelectComponent(),
                            new AgentVictoryLogic(), //+
                            new MissionHardBorderPlacer(),
                            new MissionBoundaryPlacer(),
                            new MissionBoundaryCrossingHandler(),
                            new MultiplayerPollComponent(),
                            new MultiplayerAdminComponent(),
                            new MultiplayerGameNotificationsComponent(),
                            new MissionOptionsComponent(),
                            new MissionScoreboardComponent("ClassicBattle"),
                            new AgentBattleAILogic(),
                            new AgentFadeOutLogic()
                    };
                }
                return new MissionBehaviour[]
                {
                    MissionLobbyComponent.CreateBehaviour(),
                    new MultiplayerRoundController(), //+
                    //new MultiplayerWarmupComponent(), //+
                    new MissionMultiplayerCrpgBattleClient(),
                    new MultiplayerTimerComponent(),
                    new MultiplayerMissionAgentVisualSpawnComponent(),
                    new MissionLobbyEquipmentNetworkComponent(),
                    new MultiplayerTeamSelectComponent(),
                    new AgentVictoryLogic(), //+
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new MultiplayerPollComponent(),
                    new MultiplayerGameNotificationsComponent(),
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent("ClassicBattle")
                };
            }, true, true, false);
        }
    }
}

class CrpgBattleMissionOptionsComponent : MissionOptionsComponent
{
    public new void OnAddOptionsUIHandler()
    {
        //Debug.DebugManager.Print("CrpgBattleMissionOptionsComponent::OnAddOptionsUIHandler");
        InformationManager.DisplayMessage(new InformationMessage("CrpgBattleMissionOptionsComponent::OnAddOptionsUIHandler"));
        base.OnAddOptionsUIHandler();
    }
}

[ViewCreatorModule]
public class MultiplayerMissionViews
{

	[ViewMethod("ClassicBattle")]
	public static MissionView[] OpenClassicBattle(Mission mission)
	{
	    List<MissionView> list = new List<MissionView>();
	    list.Add(ViewCreator.CreateLobbyUIHandler()); //
	    list.Add(ViewCreator.CreateMissionMultiplayerPreloadView(mission)); //
	    list.Add(ViewCreator.CreateMultiplayerTeamSelectUIHandler()); //
	    list.Add(ViewCreator.CreateMissionKillNotificationUIHandler()); //
	    list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission)); //
	    list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission)); //
	    list.Add(ViewCreator.CreateMissionMultiplayerEscapeMenu("ClassicBattle"));
	    list.Add(ViewCreator.CreateMissionScoreBoardUIHandler(mission, false)); //
	    list.Add(ViewCreator.CreateMultiplayerEndOfRoundUIHandler()); //
	    list.Add(ViewCreator.CreateLobbyEquipmentUIHandler()); //
        list.Add(ViewCreator.CreateMultiplayerFactionBanVoteUIHandler()); //++
        //list.Add(ViewCreator.CreateMultiplayerMissionOrderUIHandler(mission)); //++
        list.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission)); //
        list.Add(ViewCreator.CreateOrderTroopPlacerView(mission)); //++
        list.Add(ViewCreator.CreatePollInitiationUIHandler()); //
	    list.Add(ViewCreator.CreatePollProgressUIHandler()); //
	    list.Add(ViewCreator.CreateMissionFlagMarkerUIHandler()); //
        list.Add(ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler()); //
	    list.Add(ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(null)); //
	    list.Add(ViewCreator.CreateOptionsUIHandler()); //
	    if (!GameNetwork.IsClient)
	    {
		    list.Add(ViewCreator.CreateMultiplayerAdminPanelUIHandler());
	    }
	    list.Add(ViewCreator.CreateMissionBoundaryCrossingView()); //
	    list.Add(new MissionBoundaryWallView()); //
	    list.Add(new MissionItemContourControllerView()); //
	    list.Add(new MissionAgentContourControllerView()); //
        list.Add(new SpectatorCameraView()); //+
        return list.ToArray();
    }
}