using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade
{
	// Token: 0x020002B4 RID: 692
	public class CrpgMultiplayerMissionAgentVisualSpawnComponent : MissionNetwork
	{
		// Token: 0x14000052 RID: 82
		// (add) Token: 0x06002292 RID: 8850 RVA: 0x0007BA84 File Offset: 0x00079C84
		// (remove) Token: 0x06002293 RID: 8851 RVA: 0x0007BABC File Offset: 0x00079CBC
		public event Action OnMyAgentVisualSpawned;

		// Token: 0x14000053 RID: 83
		// (add) Token: 0x06002294 RID: 8852 RVA: 0x0007BAF4 File Offset: 0x00079CF4
		// (remove) Token: 0x06002295 RID: 8853 RVA: 0x0007BB2C File Offset: 0x00079D2C
		public event Action OnMyAgentSpawnedFromVisual;

		// Token: 0x14000054 RID: 84
		// (add) Token: 0x06002296 RID: 8854 RVA: 0x0007BB64 File Offset: 0x00079D64
		// (remove) Token: 0x06002297 RID: 8855 RVA: 0x0007BB9C File Offset: 0x00079D9C
		public event Action OnMyAgentVisualRemoved;

		// Token: 0x06002298 RID: 8856 RVA: 0x0007BBD1 File Offset: 0x00079DD1
		public override void OnBehaviourInitialize()
		{
			if (!GameNetwork.IsDedicatedServer)
			{
				NetworkCommunicator.OnPeerSynchronized += this.CreateSpawnFrameSystem;
			}
		}

		// Token: 0x06002299 RID: 8857 RVA: 0x0007BBEB File Offset: 0x00079DEB
		private void CreateSpawnFrameSystem(NetworkCommunicator networkCommunicator)
		{
			if (networkCommunicator.IsMine)
			{
				this._spawnFrameSelectionHelper = new CrpgMultiplayerMissionAgentVisualSpawnComponent.VisualSpawnFrameSelectionHelper();
				NetworkCommunicator.OnPeerSynchronized -= this.CreateSpawnFrameSystem;
			}
		}

		// Token: 0x0600229A RID: 8858 RVA: 0x0007BC14 File Offset: 0x00079E14
		public void SpawnAgentVisualsForPeer(MissionPeer missionPeer, AgentBuildData buildData, int selectedEquipmentSetIndex = -1, bool isBot = false, int totalTroopCount = 0)
		{
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			if (myPeer != null)
			{
				myPeer.GetComponent<MissionPeer>();
			}
			if (buildData.AgentVisualsIndex == 0)
			{
				missionPeer.ClearAllVisuals(false);
			}
			missionPeer.ClearVisuals(buildData.AgentVisualsIndex);
			Equipment agentOverridenSpawnEquipment = buildData.AgentOverridenSpawnEquipment;
			ItemObject item = agentOverridenSpawnEquipment[10].Item;
			MatrixFrame spawnPointFrameForPlayer = this._spawnFrameSelectionHelper.GetSpawnPointFrameForPlayer(missionPeer.Peer, buildData.AgentVisualsIndex, totalTroopCount, item != null);
			ActionIndexCache actionIndexCache = (item == null) ? SpawningBehaviourBase.PoseActionInfantry : SpawningBehaviourBase.PoseActionCavalry;
			MultiplayerClassDivisions.MPHeroClass mpheroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(buildData.AgentCharacter);
			List<MPPerkObject> allSelectedPerksForPeer = MultiplayerClassDivisions.GetAllSelectedPerksForPeer(missionPeer, mpheroClassForCharacter);
			float parameter = 0.1f + MBRandom.RandomFloat * 0.8f;
			IAgentVisual agentVisual = null;
			if (item != null)
			{
				Monster monster = item.HorseComponent.Monster;
				AgentVisualsData agentVisualsData = new AgentVisualsData().Equipment(agentOverridenSpawnEquipment).Scale(item.ScaleFactor).Frame(MatrixFrame.Identity).ActionSet(MBGlobals.GetActionSet(monster.ActionSetCode)).Scene(Mission.Current.Scene).Monster(monster).PrepareImmediately(false).UseScaledWeapons(true).HasClippingPlane(true).MountCreationKey(MountCreationKey.GetRandomMountKey(item, MBRandom.RandomInt()));
				agentVisual = Mission.Current.AgentVisualCreator.Create(agentVisualsData, "Agent " + buildData.AgentCharacter.StringId + " mount");
				MatrixFrame globalFrame = spawnPointFrameForPlayer;
				globalFrame.rotation.ApplyScaleLocal(agentVisualsData.ScaleData);
				ActionIndexCache actionIndexCache2 = ActionIndexCache.act_none;
				foreach (MPPerkObject mpperkObject in allSelectedPerksForPeer)
				{
					if (!isBot && mpperkObject.HeroMountIdleAnimOverride != null)
					{
						actionIndexCache2 = ActionIndexCache.Create(mpperkObject.HeroMountIdleAnimOverride);
						break;
					}
					if (isBot && mpperkObject.TroopMountIdleAnimOverride != null)
					{
						actionIndexCache2 = ActionIndexCache.Create(mpperkObject.TroopMountIdleAnimOverride);
						break;
					}
				}
				if (actionIndexCache2 == ActionIndexCache.act_none)
				{
					if (!isBot && !string.IsNullOrEmpty(mpheroClassForCharacter.HeroMountIdleAnim))
					{
						actionIndexCache2 = ActionIndexCache.Create(mpheroClassForCharacter.HeroMountIdleAnim);
					}
					if (isBot && !string.IsNullOrEmpty(mpheroClassForCharacter.TroopMountIdleAnim))
					{
						actionIndexCache2 = ActionIndexCache.Create(mpheroClassForCharacter.TroopMountIdleAnim);
					}
				}
				if (actionIndexCache2 != ActionIndexCache.act_none)
				{
					agentVisual.SetAction(actionIndexCache2, 0f);
					agentVisual.GetVisuals().GetSkeleton().SetAnimationParameterAtChannel(0, parameter);
					agentVisual.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.1f, globalFrame, true);
				}
				agentVisual.GetVisuals().GetEntity().SetFrame(ref globalFrame);
			}
			ActionIndexCache actionIndexCache3 = actionIndexCache;
			if (agentVisual != null)
			{
				actionIndexCache3 = agentVisual.GetVisuals().GetSkeleton().GetActionAtChannel(0);
			}
			else
			{
				foreach (MPPerkObject mpperkObject2 in allSelectedPerksForPeer)
				{
					if (!isBot && mpperkObject2.HeroIdleAnimOverride != null)
					{
						actionIndexCache3 = ActionIndexCache.Create(mpperkObject2.HeroIdleAnimOverride);
						break;
					}
					if (isBot && mpperkObject2.TroopIdleAnimOverride != null)
					{
						actionIndexCache3 = ActionIndexCache.Create(mpperkObject2.TroopIdleAnimOverride);
						break;
					}
				}
				if (actionIndexCache3 == actionIndexCache)
				{
					if (!isBot && !string.IsNullOrEmpty(mpheroClassForCharacter.HeroIdleAnim))
					{
						actionIndexCache3 = ActionIndexCache.Create(mpheroClassForCharacter.HeroIdleAnim);
					}
					if (isBot && !string.IsNullOrEmpty(mpheroClassForCharacter.TroopIdleAnim))
					{
						actionIndexCache3 = ActionIndexCache.Create(mpheroClassForCharacter.TroopIdleAnim);
					}
				}
			}
			IAgentVisual agentVisual2 = Mission.Current.AgentVisualCreator.Create(new AgentVisualsData().Equipment(agentOverridenSpawnEquipment).BodyProperties(buildData.AgentBodyProperties).Frame(spawnPointFrameForPlayer).ActionSet(MBGlobals.PlayerMaleActionSet).Scene(Mission.Current.Scene).Monster(Game.Current.HumanMonster).PrepareImmediately(false).UseMorphAnims(true).SkeletonType(buildData.AgentIsFemale ? SkeletonType.Female : SkeletonType.Male).ClothColor1(buildData.AgentClothingColor1).ClothColor2(buildData.AgentClothingColor2).AddColorRandomness(buildData.AgentVisualsIndex != 0).ActionCode(actionIndexCache3), "Mission::SpawnAgentVisuals");
			agentVisual2.SetAction(actionIndexCache3, 0f);
			agentVisual2.GetVisuals().GetSkeleton().SetAnimationParameterAtChannel(0, parameter);
			agentVisual2.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.1f, spawnPointFrameForPlayer, true);
			agentVisual2.GetVisuals().SetFrame(ref spawnPointFrameForPlayer);
			agentVisual2.SetCharacterObjectID(buildData.AgentCharacter.StringId);
			EquipmentIndex slotIndexRightHand;
			EquipmentIndex slotIndexLeftHand;
			bool flag;
			agentOverridenSpawnEquipment.GetInitialWeaponIndicesToEquip(out slotIndexRightHand, out slotIndexLeftHand, out flag);
			if (flag)
			{
				slotIndexLeftHand = EquipmentIndex.None;
			}
			agentVisual2.GetVisuals().SetWieldedWeaponIndices((int)slotIndexRightHand, (int)slotIndexLeftHand);
			PeerVisualsHolder peerVisualsHolder = new PeerVisualsHolder(missionPeer, buildData.AgentVisualsIndex, agentVisual2, agentVisual);
			missionPeer.OnVisualsSpawned(peerVisualsHolder, peerVisualsHolder.VisualsIndex);
			if (buildData.AgentVisualsIndex == 0)
			{
				missionPeer.HasSpawnedAgentVisuals = true;
				missionPeer.EquipmentUpdatingExpired = false;
			}
			if (missionPeer.IsMine && buildData.AgentVisualsIndex == 0)
			{
				Action onMyAgentVisualSpawned = this.OnMyAgentVisualSpawned;
				if (onMyAgentVisualSpawned == null)
				{
					return;
				}
				onMyAgentVisualSpawned();
			}
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x0007C100 File Offset: 0x0007A300
		public void RemoveAgentVisuals(MissionPeer missionPeer, bool sync = false)
		{
			missionPeer.ClearAllVisuals(false);
			if (!GameNetwork.IsDedicatedServer && !missionPeer.Peer.IsMine)
			{
				this._spawnFrameSelectionHelper.FreeSpawnPointFromPlayer(missionPeer.Peer);
			}
			if (sync && GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new RemoveAgentVisualsForPeer(missionPeer.GetNetworkPeer()));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
			}
			missionPeer.HasSpawnedAgentVisuals = false;
			if (this.OnMyAgentVisualRemoved != null && missionPeer.IsMine)
			{
				this.OnMyAgentVisualRemoved();
			}
			Debug.Print("Removed visuals.", 0, Debug.DebugColor.BrightWhite, 64UL);
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x0007C194 File Offset: 0x0007A394
		public void RemoveAgentVisualsWithVisualIndex(MissionPeer missionPeer, int visualsIndex, bool sync = false)
		{
			missionPeer.ClearVisuals(visualsIndex);
			if (!GameNetwork.IsDedicatedServer && visualsIndex == 0 && !missionPeer.Peer.IsMine)
			{
				this._spawnFrameSelectionHelper.FreeSpawnPointFromPlayer(missionPeer.Peer);
			}
			if (sync && GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new RemoveAgentVisualsFromIndexForPeer(missionPeer.GetNetworkPeer(), visualsIndex));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, missionPeer.GetNetworkPeer());
			}
			if (this.OnMyAgentVisualRemoved != null && missionPeer.IsMine && visualsIndex == 0)
			{
				this.OnMyAgentVisualRemoved();
			}
			Debug.Print("Removed visuals.", 0, Debug.DebugColor.BrightWhite, 64UL);
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x0007C22B File Offset: 0x0007A42B
		public void OnMyAgentSpawned()
		{
			Action onMyAgentSpawnedFromVisual = this.OnMyAgentSpawnedFromVisual;
			if (onMyAgentSpawnedFromVisual == null)
			{
				return;
			}
			onMyAgentSpawnedFromVisual();
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x0007C23D File Offset: 0x0007A43D
		public override void OnMissionTick(float dt)
		{
		}

		// Token: 0x04000C92 RID: 3218
		private CrpgMultiplayerMissionAgentVisualSpawnComponent.VisualSpawnFrameSelectionHelper _spawnFrameSelectionHelper;

		// Token: 0x0200056D RID: 1389
		private class VisualSpawnFrameSelectionHelper
		{
			// Token: 0x06003378 RID: 13176 RVA: 0x000BB7C4 File Offset: 0x000B99C4
			public VisualSpawnFrameSelectionHelper()
			{
				this._visualSpawnPoints = new GameEntity[6];
				this._visualSpawnPointUsers = new VirtualPlayer[6];
				for (int i = 0; i < 6; i++)
				{
					List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag("sp_visual_" + i).ToList<GameEntity>();
					if (list.Count > 0)
					{
						this._visualSpawnPoints[i] = list[0];
					}
				}
				this._visualSpawnPointUsers[0] = GameNetwork.MyPeer.VirtualPlayer;
			}

			// Token: 0x06003379 RID: 13177 RVA: 0x000BB84C File Offset: 0x000B9A4C
			public MatrixFrame GetSpawnPointFrameForPlayer(VirtualPlayer player, int agentVisualIndex, int totalTroopCount, bool isMounted = false)
			{
				if (agentVisualIndex == 0)
				{
					int num = -1;
					int num2 = -1;
					for (int i = 0; i < this._visualSpawnPointUsers.Length; i++)
					{
						if (this._visualSpawnPointUsers[i] == player)
						{
							num = i;
							break;
						}
						if (num2 < 0 && this._visualSpawnPointUsers[i] == null)
						{
							num2 = i;
						}
					}
					int num3 = (num >= 0) ? num : num2;
					if (num3 >= 0)
					{
						this._visualSpawnPointUsers[num3] = player;
						MatrixFrame globalFrame = this._visualSpawnPoints[num3].GetGlobalFrame();
						globalFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
						return globalFrame;
					}
					return MatrixFrame.Identity;
				}
				else
				{
					Vec3 origin = this._visualSpawnPoints[3].GetGlobalFrame().origin;
					Vec3 origin2 = this._visualSpawnPoints[1].GetGlobalFrame().origin;
					Vec3 origin3 = this._visualSpawnPoints[5].GetGlobalFrame().origin;
					Mat3 rotation = this._visualSpawnPoints[0].GetGlobalFrame().rotation;
					rotation.MakeUnit();
					List<WorldFrame> formationFramesForBeforeFormationCreation = Formation.GetFormationFramesForBeforeFormationCreation(origin2.Distance(origin3), totalTroopCount, isMounted, new WorldPosition(Mission.Current.Scene, origin), rotation);
					if (formationFramesForBeforeFormationCreation.Count < agentVisualIndex)
					{
						return new MatrixFrame(rotation, origin);
					}
					return formationFramesForBeforeFormationCreation[agentVisualIndex - 1].ToGroundMatrixFrame();
				}
			}

			// Token: 0x0600337A RID: 13178 RVA: 0x000BB974 File Offset: 0x000B9B74
			public void FreeSpawnPointFromPlayer(VirtualPlayer player)
			{
				for (int i = 0; i < this._visualSpawnPointUsers.Length; i++)
				{
					if (this._visualSpawnPointUsers[i] == player)
					{
						this._visualSpawnPointUsers[i] = null;
						return;
					}
				}
			}

			// Token: 0x04001AE5 RID: 6885
			private const string SpawnPointTagPrefix = "sp_visual_";

			// Token: 0x04001AE6 RID: 6886
			private const int NumberOfSpawnPoints = 6;

			// Token: 0x04001AE7 RID: 6887
			private const int PlayerSpawnPointIndex = 0;

			// Token: 0x04001AE8 RID: 6888
			private GameEntity[] _visualSpawnPoints;

			// Token: 0x04001AE9 RID: 6889
			private VirtualPlayer[] _visualSpawnPointUsers;
		}
	}
}
