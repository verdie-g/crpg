using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod
{
	public class CrpgBattleSpawningBehavior : SpawningBehaviourBase
	{
		public CrpgBattleSpawningBehavior()
		{
			this._spawnCheckTimer = new Timer(MBCommon.GetTime(MBCommon.TimeType.Mission), 0.2f, true);
			this.IsSpawningEnabled = true;
		}

		public override void Initialize(SpawnComponent spawnComponent)
		{
			base.Initialize(spawnComponent);
			base.OnAllAgentsFromPeerSpawnedFromVisuals += this.OnAllAgentsFromPeerSpawnedFromVisuals;
		}

		public override void Clear()
		{
			base.Clear();
			base.OnAllAgentsFromPeerSpawnedFromVisuals -= this.OnAllAgentsFromPeerSpawnedFromVisuals;
		}

		public override void OnTick(float dt)
		{
			if (this.IsSpawningEnabled && this._spawnCheckTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
			{
				this.SpawnAgents();
			}
			base.OnTick(dt);
		}

		protected override void SpawnAgents()
		{
			//var availableCultures = MBObjectManager.Instance.GetObjectTypeList<BasicCultureObject>()
			//	.Where(x => x.IsMainCulture).ToList();
			BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));

			foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
			{
				NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
				if (networkPeer.IsSynchronized && missionPeer.ControlledAgent == null && !missionPeer.HasSpawnedAgentVisuals && missionPeer.Team != null && missionPeer.Team != base.Mission.SpectatorTeam && missionPeer.SpawnTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
				{

					BasicCultureObject basicCultureObject = (missionPeer.Team.Side == BattleSideEnum.Attacker) ? @object : object2;
					MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer =
						MultiplayerClassDivisions.GetMPHeroClasses().GetRandomElement();// MultiplayerClassDivisions.GetMPHeroClassForPeer(missionPeer);

					/*	if (mpheroClassForPeer == null || mpheroClassForPeer.TroopCost > this.GameMode.GetCurrentGoldForPeer(missionPeer))
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
						{*/
					BasicCharacterObject heroCharacter = mpheroClassForPeer.HeroCharacter;
					/*Equipment equipment = heroCharacter.Equipment.Clone(false);
					List<MPPerkObject> allSelectedPerksForPeer = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, mpheroClassForPeer);
					foreach (PerkEffect perkEffect in MPPerkObject.SelectRandomPerkEffectsForPerks(true, PerkType.PerkAlternativeEquipment, allSelectedPerksForPeer))
					{
						equipment[perkEffect.NewItemIndex] = perkEffect.NewItem.EquipmentElement;
					}*/
					AgentBuildData agentBuildData = new AgentBuildData(heroCharacter);
					agentBuildData.MissionPeer(missionPeer);
					//agentBuildData.Equipment(equipment);
					//++
					agentBuildData.Equipment(Equipment.GetRandomEquipmentElements(heroCharacter, true, false, agentBuildData.AgentEquipmentSeed));
					//++
					agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(heroCharacter, 0));
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
					//}
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
				if (num < MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))
				{
					base.SpawnBot(base.Mission.AttackerTeam, @object);
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
				if (num2 < MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))
				{

					var agent = SpawnBot(base.Mission.DefenderTeam, object2);
				}
			}
		}
		protected new Agent SpawnBot(Team agentTeam, BasicCultureObject cultureLimit)
		{
			BasicCharacterObject troopCharacter = MultiplayerClassDivisions.GetMPHeroClasses(cultureLimit).ToList<MultiplayerClassDivisions.MPHeroClass>().GetRandomElement<MultiplayerClassDivisions.MPHeroClass>().TroopCharacter;
			MatrixFrame spawnFrame = this.SpawnComponent.GetSpawnFrame(agentTeam, troopCharacter.HasMount(), true);
			AgentBuildData agentBuildData = new AgentBuildData(troopCharacter);
			agentBuildData.Team(agentTeam);
			agentBuildData.InitialFrame(spawnFrame);
			agentBuildData.TroopOrigin((IAgentOriginBase)new BasicBattleAgentOrigin(troopCharacter));
			agentBuildData.EquipmentSeed(this.MissionLobbyComponent.GetRandomFaceSeedForCharacter(troopCharacter, 0));
			agentBuildData.ClothingColor1(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color : cultureLimit.ClothAlternativeColor);
			agentBuildData.ClothingColor2(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
			agentBuildData.IsFemale(troopCharacter.IsFemale);

			var randomEquipmentElements = Equipment.GetRandomEquipmentElements(troopCharacter, !(Game.Current.GameType is MultiplayerGame), false, agentBuildData.AgentEquipmentSeed);

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
			agentBuildData.Equipment(randomEquipmentElements);
			agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale, troopCharacter.GetBodyPropertiesMin(false), troopCharacter.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, troopCharacter.HairTags, troopCharacter.BeardTags, troopCharacter.TattooTags));
			Agent agent = this.Mission.SpawnAgent(agentBuildData, false, 0);

			agent.AddComponent((AgentComponent)new AgentAIStateFlagComponent(agent));
			agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
			return agent;
		}
		public override bool AllowEarlyAgentVisualsDespawning(MissionPeer lobbyPeer)
		{
			return true;
		}

		public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
		{
			/*if (this.GameMode.WarmupComponent != null && this.GameMode.WarmupComponent.IsInWarmup)
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
			return -1;*/
			return 1;
		}

		protected override bool IsRoundInProgress()
		{
			return Mission.Current.CurrentState == Mission.State.Continuing;
		}

		private new void OnAllAgentsFromPeerSpawnedFromVisuals(MissionPeer peer)
		{
			/*bool flag = peer.Team == base.Mission.AttackerTeam;
			Team defenderTeam = base.Mission.DefenderTeam;
			MultiplayerClassDivisions.MPHeroClass mpheroClass = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(flag ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))).ToList<MultiplayerClassDivisions.MPHeroClass>()[peer.SelectedTroopIndex];
			this.GameMode.ChangeCurrentGoldForPeer(peer, this.GameMode.GetCurrentGoldForPeer(peer) - mpheroClass.TroopCost);*/
		}
		private Timer _spawnCheckTimer;
	}
}
