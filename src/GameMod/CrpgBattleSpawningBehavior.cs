using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Crpg.GameMod.Api.Responses;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod
{
	public class CrpgBattleSpawningBehavior : SpawningBehaviourBase
	{
		protected event Action<MissionPeer> OnPeerSpawnedFromVisuals;
		public event SpawningBehaviourBase.OnSpawningEndedEventDelegate OnSpawningEnded;

		public CrpgBattleSpawningBehavior()
		{
			//this._spawnCheckTimer = new Timer(MBCommon.GetTime(MBCommon.TimeType.Mission), 0.2f, true);
			//this.IsSpawningEnabled = true;
			this._enforcedSpawnTimers = new List<KeyValuePair<MissionPeer, Timer>>();
		}

		public override void Initialize(SpawnComponent spawnComponent)
		{
			base.Initialize(spawnComponent);
			this._crpgBattleMissionController = base.Mission.GetMissionBehaviour<MissionMultiplayerCrpgBattle>();
			this._roundController = base.Mission.GetMissionBehaviour<MultiplayerRoundController>();
			this._roundController.OnRoundStarted += this.RequestStartSpawnSession;
			this._roundController.OnRoundEnding += this.RequestStopSpawnSession;
			this._roundController.OnRoundEnding += this.UpdateCrpgPlayer;
			/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) == 0)
			{
				this._roundController.EnableEquipmentUpdate();
			}*/
			base.OnAllAgentsFromPeerSpawnedFromVisuals += this.OnAllAgentsFromPeerSpawnedFromVisuals;
			//base.OnPeerSpawnedFromVisuals += this.OnPeerSpawnedFromVisuals;
		}

		public override void Clear()
		{
			base.Clear();
			this._roundController.OnRoundStarted -= this.RequestStartSpawnSession;
			this._roundController.OnRoundEnding -= this.RequestStopSpawnSession;
			this._roundController.OnRoundEnding -= this.UpdateCrpgPlayer;
			base.OnAllAgentsFromPeerSpawnedFromVisuals -= this.OnAllAgentsFromPeerSpawnedFromVisuals;
			//base.OnPeerSpawnedFromVisuals -= this.OnPeerSpawnedFromVisuals;
		}
		public override void OnTick(float dt)
		{
			foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
			{
				if (missionPeer.GetNetworkPeer().IsSynchronized && missionPeer.ControlledAgent == null && missionPeer.HasSpawnedAgentVisuals && !this.CanUpdateSpawnEquipment(missionPeer))
				{
					BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
					BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
					List<MPPerkObject> allSelectedPerksForPeer = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, null);
					IEnumerable<PerkEffect> enumerable = MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkDrivenPropertyBonus, allSelectedPerksForPeer);
					IEnumerable<PerkEffect> enumerable2 = MPPerkObject.SelectRandomPerkEffectsForPerks(false, PerkType.PerkDrivenPropertyBonus, allSelectedPerksForPeer);
					MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(missionPeer);
					List<MPPerkObject> allSelectedPerksForPeer2 = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, mpheroClassForPeer);
					int num = 0;
					if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0 && (this.GameMode.WarmupComponent == null || !this.GameMode.WarmupComponent.IsInWarmup))
					{
						int num2 = (int)Math.Ceiling((double)((float)MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) * mpheroClassForPeer.TroopMultiplier));
						IEnumerable<PerkEffect> enumerable3 = MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkAddTroop, allSelectedPerksForPeer2);
						int num3 = 0;
						foreach (PerkEffect perkEffect in enumerable3)
						{
							num3 += Math.Max((int)Math.Ceiling((double)((float)num2 * perkEffect.Bonus)), 1);
						}
						num += num2 + num3;
					}
					if (num > 0)
					{
						num = (int)((float)num * this.GameMode.GetTroopNumberMultiplierForMissingPlayer(missionPeer));
					}
					num++;
					for (int i = 0; i < num; i++)
					{
						bool flag = i == 0;
						BasicCharacterObject basicCharacterObject = flag ? mpheroClassForPeer.HeroCharacter : mpheroClassForPeer.TroopCharacter;
						AgentBuildData agentBuildData = new AgentBuildData(basicCharacterObject);
						if (flag)
						{
							agentBuildData.MissionPeer(missionPeer);
						}
						else
						{
							agentBuildData.OwningMissionPeer(missionPeer);
						}
						agentBuildData.VisualsIndex(i);

						//Equipment equipment = flag ? basicCharacterObject.Equipment.Clone(false) : Equipment.GetRandomEquipmentElements(basicCharacterObject, false, false, MBRandom.RandomInt());
						Equipment equipment = flag ? CreateEquipment(MissionMultiplayerCrpgBattle.CrpgGlobals.GetCrpgCharacter().Character.Items) : Equipment.GetRandomEquipmentElements(basicCharacterObject, false, false, MBRandom.RandomInt());
						/*foreach (PerkEffect perkEffect2 in MPPerkObject.SelectRandomPerkEffectsForPerks(flag, PerkType.PerkAlternativeEquipment, allSelectedPerksForPeer2))
						{
							equipment[perkEffect2.NewItemIndex] = perkEffect2.NewItem.EquipmentElement;
						}*/
						agentBuildData.Equipment(equipment);
						agentBuildData.Team(missionPeer.Team);
						agentBuildData.Formation(missionPeer.ControlledFormation);
						agentBuildData.IsFemale(flag ? missionPeer.Peer.IsFemale : basicCharacterObject.IsFemale);
						agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(basicCharacterObject));
						BasicCultureObject basicCultureObject = (missionPeer.Team == this.Mission.AttackerTeam) ? @object : object2;
						if (flag)
						{
							agentBuildData.BodyProperties(this.GetBodyProperties(missionPeer, (missionPeer.Team == this.Mission.AttackerTeam) ? @object : object2));
						}
						else
						{
							agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(basicCharacterObject, agentBuildData.AgentVisualsIndex));
							agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, basicCharacterObject.GetBodyPropertiesMin(false), basicCharacterObject.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, basicCharacterObject.HairTags, basicCharacterObject.BeardTags, basicCharacterObject.TattooTags));
						}
						agentBuildData.ClothingColor1((missionPeer.Team == this.Mission.AttackerTeam) ? basicCultureObject.Color : basicCultureObject.ClothAlternativeColor);
						agentBuildData.ClothingColor2((missionPeer.Team == this.Mission.AttackerTeam) ? basicCultureObject.Color2 : basicCultureObject.ClothAlternativeColor2);
						Banner banner = new Banner(missionPeer.Peer.BannerCode, missionPeer.Team.Color, missionPeer.Team.Color2);
						agentBuildData.Banner(banner);
						if (missionPeer.ControlledFormation != null && missionPeer.ControlledFormation.Banner == null)
						{
							missionPeer.ControlledFormation.Banner = banner;
						}
						MatrixFrame spawnFrame = this.SpawnComponent.GetSpawnFrame(missionPeer.Team, equipment[EquipmentIndex.ArmorItemEndSlot].Item != null, missionPeer.SpawnCountThisRound == 0);
						if (!spawnFrame.IsIdentity && spawnFrame != agentBuildData.AgentInitialFrame)
						{
							agentBuildData.InitialFrame(spawnFrame);
						}
						if (missionPeer.ControlledAgent != null && !flag)
						{
							MatrixFrame frame = missionPeer.ControlledAgent.Frame;
							frame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
							MatrixFrame matrixFrame = frame;
							matrixFrame.origin -= matrixFrame.rotation.f.NormalizedCopy() * 3.5f;
							Mat3 rotation = matrixFrame.rotation;
							rotation.MakeUnit();
							bool flag2 = !basicCharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].IsEmpty;
							int num4 = Math.Min(num, 10);
							List<WorldFrame> formationFramesForBeforeFormationCreation = Formation.GetFormationFramesForBeforeFormationCreation((float)num4 * Formation.GetDefaultUnitDiameter(flag2) + (float)(num4 - 1) * Formation.GetDefaultMinimumInterval(flag2), num, flag2, new WorldPosition(Mission.Current.Scene, matrixFrame.origin), rotation);
							agentBuildData.InitialFrame(formationFramesForBeforeFormationCreation[i - 1].ToGroundMatrixFrame());
						}
						Agent agent = this.Mission.SpawnAgent(agentBuildData, true, 0);
						foreach (PerkEffect perkEffect3 in ((agent.MissionPeer != null) ? enumerable : enumerable2))
						{
							agent.AddComponent(new DrivenPropertyBonusAgentComponent(agent, perkEffect3.DrivenProperty, perkEffect3.Bonus));
						}
						agent.AddComponent(new AgentAIStateFlagComponent(agent));
						if (!flag)
						{
							agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
						}
						agent.WieldInitialWeapons();
						if (flag)
						{
							Action<MissionPeer> onPeerSpawnedFromVisuals = this.OnPeerSpawnedFromVisuals;
							if (onPeerSpawnedFromVisuals != null)
							{
								onPeerSpawnedFromVisuals(missionPeer);
							}
						}
					}
					MissionPeer missionPeer2 = missionPeer;
					int spawnCountThisRound = missionPeer2.SpawnCountThisRound;
					missionPeer2.SpawnCountThisRound = spawnCountThisRound + 1;
					Action<MissionPeer> onAllAgentsFromPeerSpawnedFromVisuals = this.OnAllAgentsFromPeerSpawnedFromVisuals;
					if (onAllAgentsFromPeerSpawnedFromVisuals != null)
					{
						onAllAgentsFromPeerSpawnedFromVisuals(missionPeer);
					}
					this.AgentVisualSpawnComponent.RemoveAgentVisuals(missionPeer, true);
				}
			}
			if (!this.IsSpawningEnabled && this.IsRoundInProgress())
			{
				if (this.SpawningDelayTimer >= this.SpawningEndDelay && !this._hasCalledSpawningEnded)
				{
					Mission.Current.AllowAiTicking = true;
					if (this.OnSpawningEnded != null)
					{
						this.OnSpawningEnded();
					}
					this._hasCalledSpawningEnded = true;
				}
				this.SpawningDelayTimer += dt;
			}
			/*if (this.IsSpawningEnabled && this._spawnCheckTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
			{
				this.SpawnAgents();
			}
			base.OnTick(dt);*/
			if (this._spawningTimerTicking)
			{
				this._spawningTimer += dt;
			}
			if (this.IsSpawningEnabled)
			{
				if (!this._roundInitialSpawnOver && this.IsRoundInProgress())
				{
					foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
					{
						MissionPeer component = networkPeer.GetComponent<MissionPeer>();
						if (((component != null) ? component.Team : null) != null && component.Team.Side != BattleSideEnum.None)
						{
							this.SpawnComponent.SetEarlyAgentVisualsDespawning(component, true);
							InformationManager.DisplayMessage(new InformationMessage("SpawnComponent :: SetEarlyAgentVisualsDespawning"));
						}
					}
					this._roundInitialSpawnOver = true;
					base.Mission.AllowAiTicking = true;
				}
				//InformationManager.DisplayMessage(new InformationMessage("OnTick :: _roundInitialSpawnOver = " + this._roundInitialSpawnOver + " IsRoundInProgress = " + this.IsRoundInProgress()));
				this.SpawnAgents();
				//if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0 && this._spawningTimer > (float)MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))
				if (this._spawningTimer > (float)MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))
				{
					this.IsSpawningEnabled = false;
					this._spawningTimer = 0f;
					this._spawningTimerTicking = false;
				}
			}
			//base.OnTick(dt);
		}
		public override void RequestStartSpawnSession()
		{
			if (!this.IsSpawningEnabled)
			{
				Mission.Current.SetBattleAgentCount(-1);
				this.IsSpawningEnabled = true;
				this._haveBotsBeenSpawned = false;
				this._spawningTimerTicking = true;
				base.ResetSpawnCounts();
				base.ResetSpawnTimers();
			}
		}
		public void UpdateCrpgPlayer()
		{
			// mise a jour joueur
			_crpgBattleMissionController.CreateAndTick();
			InformationManager.DisplayMessage(new InformationMessage("Spawn :: UpdateCrpgPlayer"));
		}

		protected override void SpawnAgents()
		{
			BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			if (!this._haveBotsBeenSpawned && (MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0 || MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0))
			{
				InformationManager.DisplayMessage(new InformationMessage("Spawn !_haveBotsBeenSpawned?"));

				Mission.Current.AllowAiTicking = false;
				/*List<string> list = new List<string>
				{
					"11.8.1.4345.4345.770.774.1.0.0.133.7.5.512.512.784.769.1.0.0",
					"11.8.1.4345.4345.770.774.1.0.0.156.7.5.512.512.784.769.1.0.0",
					"11.8.1.4345.4345.770.774.1.0.0.155.7.5.512.512.784.769.1.0.0",
					"11.8.1.4345.4345.770.774.1.0.0.158.7.5.512.512.784.769.1.0.0",
					"11.8.1.4345.4345.770.774.1.0.0.118.7.5.512.512.784.769.1.0.0",
					"11.8.1.4345.4345.770.774.1.0.0.149.7.5.512.512.784.769.1.0.0"
				};*/
				foreach (Team team in base.Mission.Teams)
				{
					if (base.Mission.AttackerTeam == team || base.Mission.DefenderTeam == team)
					{
						BasicCultureObject basicCultureObject = (team == base.Mission.AttackerTeam) ? @object : object2;
						int num = (base.Mission.AttackerTeam == team) ? MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
						int num2 = 0;
						for (int i = 0; i < num; i++)
						{
							Formation formation = null;
							/*
							if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
							{
								while (formation == null || formation.PlayerOwner != null)
								{
									FormationClass formationClass = (FormationClass)num2;
									formation = team.GetFormation(formationClass);
									num2++;
								}
							}
							if (formation != null)
							{
								formation.BannerCode = list[num2 - 1];
							}*/
							MultiplayerClassDivisions.MPHeroClass randomElement = MultiplayerClassDivisions.GetMPHeroClasses(basicCultureObject).ToList<MultiplayerClassDivisions.MPHeroClass>().GetRandomElement<MultiplayerClassDivisions.MPHeroClass>();
							BasicCharacterObject heroCharacter = randomElement.HeroCharacter;
							BasicCharacterObject troopCharacter = randomElement.TroopCharacter;
							AgentBuildData agentBuildData = new AgentBuildData(heroCharacter);
							agentBuildData.Equipment(randomElement.HeroCharacter.Equipment);
							//Equipment equipment = CreateEquipment(MissionMultiplayerCrpgBattle.CrpgGlobals.GetCrpgCharacter().Character.Items);
							//agentBuildData.Equipment(equipment);
							agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(heroCharacter));
							agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(heroCharacter, 0));
							agentBuildData.Team(team);
							agentBuildData.VisualsIndex(0);
							if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) == 0)
							{
								//agentBuildData.InitialFrame(this.SpawnComponent.GetSpawnFrame(team, randomElement.HeroCharacter.Equipment[EquipmentIndex.ArmorItemEndSlot].Item != null, true));
								agentBuildData.InitialFrame(this.SpawnComponent.GetSpawnFrame(team, troopCharacter.HasMount(), true));
							}
							agentBuildData.Formation(formation);
							agentBuildData.SpawnOnInitialPoint(true);
							agentBuildData.IsFemale(heroCharacter.IsFemale);
							agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, heroCharacter.GetBodyPropertiesMin(false), heroCharacter.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, heroCharacter.HairTags, heroCharacter.BeardTags, heroCharacter.TattooTags));
							agentBuildData.ClothingColor1((team.Side == BattleSideEnum.Attacker) ? basicCultureObject.Color : basicCultureObject.ClothAlternativeColor);
							agentBuildData.ClothingColor2((team.Side == BattleSideEnum.Attacker) ? basicCultureObject.Color2 : basicCultureObject.ClothAlternativeColor2);
							Agent agent = base.Mission.SpawnAgent(agentBuildData, false, 0);
							agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
							agent.WieldInitialWeapons();
							/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
							{
								int num3 = (int)Math.Ceiling((double)((float)MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) * randomElement.TroopMultiplier));
								for (int j = 0; j < num3; j++)
								{
									this.SpawnBotInBotFormation(j + 1, team, basicCultureObject, randomElement.TroopCharacter.StringId, formation);
								}
								this.BotFormationSpawned(team);
								formation.IsAIControlled = true;
							}*/
						}
						if (num > 0 && team.Formations.Any<Formation>())
						{
							TeamAIGeneral teamAIGeneral = new TeamAIGeneral(Mission.Current, team, 10f, 1f);
							teamAIGeneral.AddTacticOption(new TacticSergeantMPBotTactic(team));
							team.AddTeamAI(teamAIGeneral, false);
						}
					}
				}
				//this.AllBotFormationsSpawned();
				this._haveBotsBeenSpawned = true;
			}
			foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
			{
				NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
				if (networkPeer.IsSynchronized && missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None && (!this.CheckIfEnforcedSpawnTimerExpiredForPeer(missionPeer)))
				//if (networkPeer.IsSynchronized && missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None && (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) != 0 || !this.CheckIfEnforcedSpawnTimerExpiredForPeer(missionPeer)))
				{
					Team team2 = missionPeer.Team;
					bool flag = team2 == base.Mission.AttackerTeam;
					Team defenderTeam = base.Mission.DefenderTeam;
					BasicCultureObject basicCultureObject2 = flag ? @object : object2;
					MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(missionPeer);
					if (missionPeer.ControlledAgent == null && !missionPeer.HasSpawnedAgentVisuals && missionPeer.Team != null && missionPeer.Team != base.Mission.SpectatorTeam && missionPeer.SpawnTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
					{
						InformationManager.DisplayMessage(new InformationMessage("Spawn Visual"));
						int currentGoldForPeer = this._crpgBattleMissionController.GetCurrentGoldForPeer(missionPeer);
						if (mpheroClassForPeer == null || (this._crpgBattleMissionController.UseGold() && mpheroClassForPeer.TroopCost > currentGoldForPeer))
						{
							if (currentGoldForPeer >= MultiplayerClassDivisions.GetMinimumTroopCost(basicCultureObject2) && missionPeer.SelectedTroopIndex != 0)
							{
								missionPeer.SelectedTroopIndex = 0;
								GameNetwork.BeginBroadcastModuleEvent();
								GameNetwork.WriteMessage(new UpdateSelectedTroopIndex(networkPeer, 0));
								GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, networkPeer);
							}
						}
						else
						{
							/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) == 0)
							{
								this.CreateEnforcedSpawnTimerForPeer(missionPeer, 15);
							}*/
							//Formation formation2 = missionPeer.ControlledFormation;
							/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0 && formation2 == null)
							{
								FormationClass formationIndex = missionPeer.Team.FormationsIncludingEmpty.First((Formation x) => x.PlayerOwner == null && !x.ContainsAgentVisuals && !x.Units.Any()).FormationIndex;
								formation2 = team2.GetFormation(formationIndex);
								formation2.ContainsAgentVisuals = true;
								if (formation2.BannerCode.IsStringNoneOrEmpty())
								{
									formation2.BannerCode = missionPeer.Peer.BannerCode;
								}
							}*/
							List<MPPerkObject> allSelectedPerksForPeer = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, mpheroClassForPeer);
							BasicCharacterObject heroCharacter2 = mpheroClassForPeer.HeroCharacter;

							AgentBuildData agentBuildData2 = new AgentBuildData(heroCharacter2);
							agentBuildData2.MissionPeer(missionPeer);

							bool gcsync = false;
							while (gcsync != true)
							{
								InformationManager.DisplayMessage(new InformationMessage("Spawn gcsync?"));
								var gc = MissionMultiplayerCrpgBattle.CrpgGlobals.GetCrpgCharacter().Character.Items;
								if(gc != null)
								{
									InformationManager.DisplayMessage(new InformationMessage("Spawn gcsync OK!?"));
									gcsync = true;
								}
							}
							Equipment equipment = CreateEquipment(MissionMultiplayerCrpgBattle.CrpgGlobals.GetCrpgCharacter().Character.Items);

							//Equipment equipment = heroCharacter2.Equipment.Clone(false);
							//Equipment equipment = CreateEquipment(CrpgGlobals.GetCrpgCharacter().Character.Items);
							//var equipment = new Equipment();

							/*foreach (PerkEffect perkEffect in MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkAlternativeEquipment, allSelectedPerksForPeer))
							{
								equipment[perkEffect.NewItemIndex] = perkEffect.NewItem.EquipmentElement;
							}*/
							int amountOfAgentVisualsForPeer = missionPeer.GetAmountOfAgentVisualsForPeer();
							bool flag2 = amountOfAgentVisualsForPeer > 0;
							agentBuildData2.Equipment(equipment);
							//this.AgentOverridenEquipment = equipment;
							agentBuildData2.Team(missionPeer.Team);
							agentBuildData2.VisualsIndex(0);
							//if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) == 0)
							//{
								if (!flag2)
								{
									agentBuildData2.InitialFrame(this.SpawnComponent.GetSpawnFrame(missionPeer.Team, equipment[EquipmentIndex.ArmorItemEndSlot].Item != null, true));
								}
								else
								{
									MatrixFrame frame = missionPeer.GetAgentVisualForPeer(0).GetFrame();
									frame.rotation.MakeUnit();
									agentBuildData2.InitialFrame(frame);
								}
							//}
							//agentBuildData2.Formation(formation2);
							agentBuildData2.SpawnOnInitialPoint(true);
							agentBuildData2.MakeUnitStandOutOfFormationDistance(7f);
							agentBuildData2.IsFemale(missionPeer.Peer.IsFemale);
							BodyProperties bodyProperties = base.GetBodyProperties(missionPeer, (missionPeer.Team == base.Mission.AttackerTeam) ? @object : object2);
							agentBuildData2.BodyProperties(bodyProperties);
							agentBuildData2.ClothingColor1((team2 == base.Mission.AttackerTeam) ? basicCultureObject2.Color : basicCultureObject2.ClothAlternativeColor);
							agentBuildData2.ClothingColor2((team2 == base.Mission.AttackerTeam) ? basicCultureObject2.Color2 : basicCultureObject2.ClothAlternativeColor2);


							if (this.GameMode.ShouldSpawnVisualsForServer(networkPeer))
							{
								InformationManager.DisplayMessage(new InformationMessage("ShouldSpawnVisualsForServer"));
								base.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData2, missionPeer.SelectedTroopIndex, false, 0);
								//base.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData2, 0, false, 0);
							}
							InformationManager.DisplayMessage(new InformationMessage("HandleAgentVisualSpawning"));
							this.GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData2, 0);
							//missionPeer.ControlledFormation = formation2;
							/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
							{
								int num4 = (int)Math.Ceiling((double)((float)MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) * mpheroClassForPeer.TroopMultiplier));
								int num5 = 0;
								foreach (PerkEffect perkEffect2 in MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkAddTroop, allSelectedPerksForPeer))
								{
									num5 += Math.Max((int)Math.Ceiling((double)((float)num4 * perkEffect2.Bonus)), 1);
								}
								for (int k = 0; k < num4; k++)
								{
									if (k + 1 >= amountOfAgentVisualsForPeer)
									{
										flag2 = false;
									}
									this.SpawnBotVisualsInPlayerFormation(missionPeer, k + 1, team2, basicCultureObject2, mpheroClassForPeer.TroopCharacter.StringId, formation2, flag2, allSelectedPerksForPeer, num4 + num5);
								}
								if (num5 > 0)
								{
									for (int l = num4; l < num4 + num5; l++)
									{
										if (l + 1 >= amountOfAgentVisualsForPeer)
										{
											flag2 = false;
										}
										this.SpawnBotVisualsInPlayerFormation(missionPeer, l + 1, team2, basicCultureObject2, mpheroClassForPeer.TroopCharacter.StringId, formation2, flag2, allSelectedPerksForPeer, num4 + num5);
									}
								}
							}*/
						}
					}
				}
			}
		}
		

		private Equipment CreateEquipment(GameCharacterItems gc)
		{
			var objectManager = Game.Current.ObjectManager;

			var equipment = new Equipment(false);
			SetEquipmentSlot(equipment, EquipmentIndex.Head, gc.HeadItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Cape, gc.CapeItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Body, gc.BodyItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Gloves, gc.HandItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Leg, gc.LegItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.HorseHarness, gc.HorseHarnessItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Horse, gc.HorseItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Weapon1, gc.Weapon1ItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Weapon2, gc.Weapon2ItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Weapon3, gc.Weapon3ItemMbId, objectManager);
			SetEquipmentSlot(equipment, EquipmentIndex.Weapon4, gc.Weapon4ItemMbId, objectManager);
			return equipment;
		}

		private void SetEquipmentSlot(Equipment equipment, EquipmentIndex slot, string? itemId, MBObjectManager objectManager)
		{
			if (itemId == null)
			{
				return;
			}

			ItemObject item = objectManager.GetObject<ItemObject>(itemId);
			equipment[slot] = new EquipmentElement(item, new ItemModifier());
		}
		/*private new void OnPeerSpawnedFromVisuals(MissionPeer peer)
		{
			if (peer.ControlledFormation != null)
			{
				peer.ControlledAgent.Team.AssignPlayerAsSergeantOfFormation(peer, peer.ControlledFormation.FormationIndex);
			}
		}*/
		private new void OnAllAgentsFromPeerSpawnedFromVisuals(MissionPeer peer)
		{
			InformationManager.DisplayMessage(new InformationMessage("OnAllAgentsFromPeerSpawnedFromVisuals"));
			/*if (peer.ControlledFormation != null)
			{
				peer.ControlledFormation.OnFormationDispersed();
				peer.ControlledFormation.MovementOrder = MovementOrder.MovementOrderFollow(peer.ControlledAgent);
				NetworkCommunicator networkPeer = peer.GetNetworkPeer();
				if (peer.BotsUnderControlAlive != 0 || peer.BotsUnderControlTotal != 0)
				{
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage(new BotsControlledChange(networkPeer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
					base.Mission.GetMissionBehaviour<MissionMultiplayerCrpgBattleClient>().OnBotsControlledChanged(peer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal);
				}
				if (peer.Team == base.Mission.AttackerTeam)
				{
					base.Mission.NumOfFormationsSpawnedTeamOne++;
				}
				else
				{
					base.Mission.NumOfFormationsSpawnedTeamTwo++;
				}
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new SetSpawnedFormationCount(base.Mission.NumOfFormationsSpawnedTeamOne, base.Mission.NumOfFormationsSpawnedTeamTwo));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
			}*/
			if (this._crpgBattleMissionController.UseGold())
			{
				bool flag = peer.Team == base.Mission.AttackerTeam;
				Team defenderTeam = base.Mission.DefenderTeam;
				MultiplayerClassDivisions.MPHeroClass mpheroClass = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(flag ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))).ToList<MultiplayerClassDivisions.MPHeroClass>()[peer.SelectedTroopIndex];
				this._crpgBattleMissionController.ChangeCurrentGoldForPeer(peer, this._crpgBattleMissionController.GetCurrentGoldForPeer(peer) - mpheroClass.TroopCost);
				//this._crpgBattleMissionController.ChangeCurrentGoldForPeer(peer, this._crpgBattleMissionController.GetCurrentGoldForPeer(peer) - 300);
			}
		}
		/*private void BotFormationSpawned(Team team)
		{
			if (team == base.Mission.AttackerTeam)
			{
				base.Mission.NumOfFormationsSpawnedTeamOne++;
				return;
			}
			if (team == base.Mission.DefenderTeam)
			{
				base.Mission.NumOfFormationsSpawnedTeamTwo++;
			}
		}*/

		// Token: 0x06002156 RID: 8534 RVA: 0x00074658 File Offset: 0x00072858
		/*private void AllBotFormationsSpawned()
		{
			if (base.Mission.NumOfFormationsSpawnedTeamOne != 0 || base.Mission.NumOfFormationsSpawnedTeamTwo != 0)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new SetSpawnedFormationCount(base.Mission.NumOfFormationsSpawnedTeamOne, base.Mission.NumOfFormationsSpawnedTeamTwo));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
			}
		}*/

		// Token: 0x06002157 RID: 8535 RVA: 0x000746AC File Offset: 0x000728AC
		public override bool AllowEarlyAgentVisualsDespawning(MissionPeer lobbyPeer)
		{
			/*if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) != 0)
			{
				return false;
			}
			if (!this._roundController.IsRoundInProgress)
			{
				return false;
			}
			if (!lobbyPeer.HasSpawnTimerExpired && lobbyPeer.SpawnTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
			{
				lobbyPeer.HasSpawnTimerExpired = true;
			}
			return lobbyPeer.HasSpawnTimerExpired;*/
			return false;
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x000746FC File Offset: 0x000728FC
		protected override bool IsRoundInProgress()
		{
			return this._roundController.IsRoundInProgress;
		}

		// Token: 0x06002159 RID: 8537 RVA: 0x0007470C File Offset: 0x0007290C
		private void CreateEnforcedSpawnTimerForPeer(MissionPeer peer, int durationInSeconds)
		{
			InformationManager.DisplayMessage(new InformationMessage("CreateEnforcedSpawnTimerForPeer :: "+ durationInSeconds));

			if (this._enforcedSpawnTimers.Any((KeyValuePair<MissionPeer, Timer> pair) => pair.Key == peer))
			{
				return;
			}
			this._enforcedSpawnTimers.Add(new KeyValuePair<MissionPeer, Timer>(peer, new Timer(MBCommon.GetTime(MBCommon.TimeType.Mission), (float)durationInSeconds, true)));
			Debug.Print(string.Concat(new object[]
			{
				"EST for ",
				peer.Name,
				" set to ",
				durationInSeconds,
				" seconds."
			}), 0, Debug.DebugColor.Yellow, 64UL);
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x000747AC File Offset: 0x000729AC
		private bool CheckIfEnforcedSpawnTimerExpiredForPeer(MissionPeer peer)
		{
			KeyValuePair<MissionPeer, Timer> keyValuePair = this._enforcedSpawnTimers.FirstOrDefault((KeyValuePair<MissionPeer, Timer> pr) => pr.Key == peer);
			if (keyValuePair.Key == null)
			{
				return false;
			}
			if (peer.ControlledAgent != null)
			{
				this._enforcedSpawnTimers.RemoveAll((KeyValuePair<MissionPeer, Timer> p) => p.Key == peer);
				Debug.Print("EST for " + peer.Name + " is no longer valid (spawned already).", 0, Debug.DebugColor.Yellow, 64UL);
				InformationManager.DisplayMessage(new InformationMessage("EST for " + peer.Name + " is no longer valid (spawned already)."));
				return false;
			}
			Timer value = keyValuePair.Value;
			if (peer.HasSpawnedAgentVisuals && value.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
			{
				this.SpawnComponent.SetEarlyAgentVisualsDespawning(peer, true);
				this._enforcedSpawnTimers.RemoveAll((KeyValuePair<MissionPeer, Timer> p) => p.Key == peer);
				Debug.Print("EST for " + peer.Name + " has expired.", 0, Debug.DebugColor.Yellow, 64UL);
				InformationManager.DisplayMessage(new InformationMessage("EST for " + peer.Name + " has expired."));
				return true;
			}
			return false;
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x000748AA File Offset: 0x00072AAA
		public override void OnClearScene()
		{
			InformationManager.DisplayMessage(new InformationMessage("OnClearScene"));
			base.OnClearScene();
			this._enforcedSpawnTimers.Clear();
			this._roundInitialSpawnOver = false;
		}

		// Token: 0x0600215C RID: 8540 RVA: 0x000748C4 File Offset: 0x00072AC4
		/*protected void SpawnBotInBotFormation(int visualsIndex, Team agentTeam, BasicCultureObject cultureLimit, string troopName, Formation formation)
		{
			BasicCharacterObject @object = MBObjectManager.Instance.GetObject<BasicCharacterObject>(troopName);
			AgentBuildData agentBuildData = new AgentBuildData(@object);
			agentBuildData.Team(agentTeam);
			agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(@object));
			agentBuildData.VisualsIndex(visualsIndex);
			agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(@object, visualsIndex));
			agentBuildData.Equipment(Equipment.GetRandomEquipmentElements(@object, !(Game.Current.GameType is MultiplayerGame), false, agentBuildData.AgentEquipmentSeed));
			agentBuildData.SpawnOnInitialPoint(true);
			agentBuildData.Formation(formation);
			agentBuildData.ClothingColor1((agentTeam.Side == BattleSideEnum.Attacker) ? cultureLimit.Color : cultureLimit.ClothAlternativeColor);
			agentBuildData.ClothingColor2((agentTeam.Side == BattleSideEnum.Attacker) ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
			agentBuildData.IsFemale(@object.IsFemale);
			agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, @object.GetBodyPropertiesMin(false), @object.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, @object.HairTags, @object.BeardTags, @object.TattooTags));
			Agent agent = base.Mission.SpawnAgent(agentBuildData, false, 0);
			agent.AddComponent(new AgentAIStateFlagComponent(agent));
			agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
		}*/

		// Token: 0x0600215D RID: 8541 RVA: 0x000749FC File Offset: 0x00072BFC
		/*protected void SpawnBotVisualsInPlayerFormation(MissionPeer missionPeer, int visualsIndex, Team agentTeam, BasicCultureObject cultureLimit, string troopName, Formation formation, bool updateExistingAgentVisuals, List<MPPerkObject> perks, int totalCount)
		{
			BasicCharacterObject @object = MBObjectManager.Instance.GetObject<BasicCharacterObject>(troopName);
			AgentBuildData agentBuildData = new AgentBuildData(@object);
			agentBuildData.Team(agentTeam);
			agentBuildData.OwningMissionPeer(missionPeer);
			agentBuildData.VisualsIndex(visualsIndex);
			Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements(@object, !(Game.Current.GameType is MultiplayerGame), false, MBRandom.RandomInt());
			foreach (PerkEffect perkEffect in MPPerkObject.SelectRandomPerkEffectsForPerks(false, PerkType.PerkAlternativeEquipment, perks))
			{
				randomEquipmentElements[perkEffect.NewItemIndex] = perkEffect.NewItem.EquipmentElement;
			}
			agentBuildData.Equipment(randomEquipmentElements);
			agentBuildData.SpawnOnInitialPoint(true);
			agentBuildData.Formation(formation);
			agentBuildData.ClothingColor1((agentTeam.Side == BattleSideEnum.Attacker) ? cultureLimit.Color : cultureLimit.ClothAlternativeColor);
			agentBuildData.ClothingColor2((agentTeam.Side == BattleSideEnum.Attacker) ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
			agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(@object));
			agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(@object, visualsIndex));
			agentBuildData.IsFemale(@object.IsFemale);
			agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, @object.GetBodyPropertiesMin(false), @object.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, @object.HairTags, @object.BeardTags, @object.TattooTags));
			NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
			if (this.GameMode.ShouldSpawnVisualsForServer(networkPeer))
			{
				base.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData, -1, true, totalCount);
			}
			this.GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData, totalCount);
		}*/
		/*protected override void SpawnAgents()
		{
			BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
			{
				NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
				if (networkPeer.IsSynchronized && missionPeer.ControlledAgent == null && !missionPeer.HasSpawnedAgentVisuals && missionPeer.Team != null && missionPeer.Team != base.Mission.SpectatorTeam && missionPeer.SpawnTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
				{
					BasicCultureObject basicCultureObject = (missionPeer.Team.Side == BattleSideEnum.Attacker) ? @object : object2;
					MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer =
					MultiplayerClassDivisions.GetMPHeroClassForPeer(missionPeer);//MultiplayerClassDivisions.GetMPHeroClasses().GetRandomElement();

					if (mpheroClassForPeer == null || mpheroClassForPeer.TroopCost > this.GameMode.GetCurrentGoldForPeer(missionPeer))
					{
						if (missionPeer.SelectedTroopIndex != 0)
						{
							missionPeer.SelectedTroopIndex = 0;
							GameNetwork.BeginBroadcastModuleEvent();
							GameNetwork.WriteMessage(new UpdateSelectedTroopIndex(networkPeer, 0));
							GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, networkPeer);
						}
					}
					else
					{
					BasicCharacterObject heroCharacter = mpheroClassForPeer.HeroCharacter;
					Equipment equipment = heroCharacter.Equipment.Clone(false);
					List<MPPerkObject> allSelectedPerksForPeer = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, mpheroClassForPeer);
					foreach (PerkEffect perkEffect in MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkAlternativeEquipment, allSelectedPerksForPeer))
					{
						equipment[perkEffect.NewItemIndex] = perkEffect.NewItem.EquipmentElement;
					}
					AgentBuildData agentBuildData = new AgentBuildData(heroCharacter);
					agentBuildData.MissionPeer(missionPeer);
					agentBuildData.Equipment(equipment);
					//++
					//agentBuildData.Equipment(Equipment.GetRandomEquipmentElements(heroCharacter, true, false, agentBuildData.AgentEquipmentSeed));
					//++
					//agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(heroCharacter, 0));
					agentBuildData.Team(missionPeer.Team);
					agentBuildData.IsFemale(missionPeer.Peer.IsFemale);
					agentBuildData.BodyProperties(base.GetBodyProperties(missionPeer, (missionPeer.Team == base.Mission.AttackerTeam) ? @object : object2));
					agentBuildData.VisualsIndex(0);
					//agentBuildData.ClothingColor1(availableCultures.GetRandomElement().Color);
					//agentBuildData.ClothingColor2(availableCultures.GetRandomElement().Color2);
					agentBuildData.ClothingColor1((missionPeer.Team == base.Mission.AttackerTeam) ? basicCultureObject.Color : basicCultureObject.ClothAlternativeColor);
					agentBuildData.ClothingColor2((missionPeer.Team == base.Mission.AttackerTeam) ? basicCultureObject.Color2 : basicCultureObject.ClothAlternativeColor2);
					agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(heroCharacter));
					if (this.GameMode.ShouldSpawnVisualsForServer(networkPeer))
					{
						base.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData, missionPeer.SelectedTroopIndex, false, 0);
					}
					this.GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData, 0);
					}
				}
			}
			if (base.Mission.AttackerTeam != null)
			{
				int num = 0;
				foreach (Agent agent in base.Mission.AttackerTeam.ActiveAgents)
				{
					if (agent.Character != null && agent.MissionPeer == null)
					{
						num++;
					}
				}
				int num1bot = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
				if (num < num1bot)
				{
					base.SpawnBot(base.Mission.AttackerTeam, @object);
					MultiplayerOptions.OptionType.NumberOfBotsTeam1.SetValue(num1bot - 1, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
					InformationManager.DisplayMessage(new InformationMessage("NumberOfBotsTeam1"+ num1bot));

				}
			}
			if (base.Mission.DefenderTeam != null)
			{
				int num2 = 0;
				foreach (Agent agent2 in base.Mission.DefenderTeam.ActiveAgents)
				{
					if (agent2.Character != null && agent2.MissionPeer == null)
					{
						num2++;
					}
				}
				int num2bot = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);

				if (num2 < num2bot)
				{
					base.SpawnBot(base.Mission.DefenderTeam, object2);
					MultiplayerOptions.OptionType.NumberOfBotsTeam2.SetValue(num2bot - 1, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
					InformationManager.DisplayMessage(new InformationMessage("NumberOfBotsTeam2" + num2bot));
				}
			}
		}*/
		/*protected new Agent SpawnBot(Team agentTeam, BasicCultureObject cultureLimit)
		{
			BasicCharacterObject troopCharacter = MultiplayerClassDivisions.GetMPHeroClasses(cultureLimit).ToList<MultiplayerClassDivisions.MPHeroClass>().GetRandomElement<MultiplayerClassDivisions.MPHeroClass>().TroopCharacter;
			MatrixFrame spawnFrame = this.SpawnComponent.GetSpawnFrame(agentTeam, troopCharacter.HasMount(), true);
			AgentBuildData agentBuildData = new AgentBuildData(troopCharacter);
			agentBuildData.Team(agentTeam);
			agentBuildData.InitialFrame(spawnFrame);
			agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(troopCharacter));
			agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(troopCharacter, 0));
			agentBuildData.ClothingColor1(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color : cultureLimit.ClothAlternativeColor);
			agentBuildData.ClothingColor2(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
			agentBuildData.IsFemale(troopCharacter.IsFemale);
			agentBuildData.Equipment(Equipment.GetRandomEquipmentElements(troopCharacter, !(Game.Current.GameType is MultiplayerGame), false, agentBuildData.AgentEquipmentSeed));
			*/
			/*var randomEquipmentElements = Equipment.GetRandomEquipmentElements(troopCharacter, !(Game.Current.GameType is MultiplayerGame), false, agentBuildData.AgentEquipmentSeed);

			var items = new Dictionary<ItemObject.ItemTypeEnum, ItemObject>();
			foreach (ItemObject.ItemTypeEnum itemtype in ((ItemObject.ItemTypeEnum[])Enum.GetValues(
				typeof(ItemObject.ItemTypeEnum))).Skip(1))
			{
				switch (itemtype)
				{
					case ItemObject.ItemTypeEnum.Goods:
					case ItemObject.ItemTypeEnum.Pistol:
					case ItemObject.ItemTypeEnum.Bullets:
					case ItemObject.ItemTypeEnum.Musket:
					case ItemObject.ItemTypeEnum.Animal:
					case ItemObject.ItemTypeEnum.Banner:
					case ItemObject.ItemTypeEnum.Book:
					case ItemObject.ItemTypeEnum.ChestArmor:
					case ItemObject.ItemTypeEnum.Invalid:
						continue;

				}
				items[itemtype] = ItemObject.All
					.Where(x => x.ItemType == itemtype).GetRandomElement();
			}

			randomEquipmentElements[EquipmentIndex.Weapon0] = new EquipmentElement(items[ItemObject.ItemTypeEnum.OneHandedWeapon], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Weapon1] = new EquipmentElement(items[ItemObject.ItemTypeEnum.Shield], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Weapon2] = new EquipmentElement(items[ItemObject.ItemTypeEnum.Bow], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Weapon3] = new EquipmentElement(items[ItemObject.ItemTypeEnum.Arrows], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Weapon4] = new EquipmentElement(items[ItemObject.ItemTypeEnum.TwoHandedWeapon], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Body] = new EquipmentElement(items[ItemObject.ItemTypeEnum.BodyArmor], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Cape] = new EquipmentElement(items[ItemObject.ItemTypeEnum.Cape], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Gloves] = new EquipmentElement(items[ItemObject.ItemTypeEnum.HandArmor], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Head] = new EquipmentElement(items[ItemObject.ItemTypeEnum.HeadArmor], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Leg] = new EquipmentElement(items[ItemObject.ItemTypeEnum.LegArmor], new ItemModifier());
			randomEquipmentElements[EquipmentIndex.Horse] = new EquipmentElement(items[ItemObject.ItemTypeEnum.Horse], new ItemModifier());
			agentBuildData.Equipment(randomEquipmentElements);*/
			/*agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, troopCharacter.GetBodyPropertiesMin(false), troopCharacter.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, troopCharacter.HairTags, troopCharacter.BeardTags, troopCharacter.TattooTags));
			Agent agent = this.Mission.SpawnAgent(agentBuildData, false, 0);
			agent.AddComponent((AgentComponent)new AgentAIStateFlagComponent(agent));
			agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
			return agent;
		}*/
		/*public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
		{
			if (this.GameMode.WarmupComponent != null && this.GameMode.WarmupComponent.IsInWarmup)
			{
				return 3;
			}
			if (peer.Team != null)
			{
				if (peer.Team.Side == BattleSideEnum.Attacker)
				{
					return MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
				}
				if (peer.Team.Side == BattleSideEnum.Defender)
				{
					return MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
				}
			}
			return -1;
			//return 1;
		}*/

		

		
		//private Timer _spawnCheckTimer;

		private const int EnforcedSpawnTimeInSeconds = 15;

		// Token: 0x04000C65 RID: 3173
		private float _spawningTimer;

		// Token: 0x04000C66 RID: 3174
		private bool _spawningTimerTicking;

		// Token: 0x04000C67 RID: 3175
		private bool _haveBotsBeenSpawned;

		// Token: 0x04000C68 RID: 3176
		private bool _roundInitialSpawnOver;

		// Token: 0x04000C69 RID: 3177
		private MissionMultiplayerCrpgBattle _crpgBattleMissionController;

		// Token: 0x04000C6A RID: 3178
		private MultiplayerRoundController _roundController;

		// Token: 0x04000C6B RID: 3179
		private List<KeyValuePair<MissionPeer, Timer>> _enforcedSpawnTimers;

		// Token: 0x04000C1E RID: 3102
		private bool _hasCalledSpawningEnded;
		public delegate void OnSpawningEndedEventDelegate();
	}
}
