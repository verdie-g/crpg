using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI
{
	// Token: 0x0200001D RID: 29
	[OverrideView(typeof(MissionLobbyEquipmentUIHandler))]
	public class CrpgMissionGauntletClassLoadout : MissionView
	{
		// Token: 0x060000F5 RID: 245 RVA: 0x000068D4 File Offset: 0x00004AD4
		public override void OnMissionScreenInitialize()
		{
			base.OnMissionScreenInitialize();
			this.ViewOrderPriorty = 20;
			this._missionLobbyEquipmentNetworkComponent = base.Mission.GetMissionBehaviour<MissionLobbyEquipmentNetworkComponent>();
			this._gameModeClient = base.Mission.GetMissionBehaviour<MissionMultiplayerGameModeBaseClient>();
			MissionPeer.OnTeamChanged += this.OnTeamChanged;
			this._missionLobbyEquipmentNetworkComponent.OnToggleLoadout += this.OnTryToggle;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00006939 File Offset: 0x00004B39
		public override void OnMissionScreenDeactivate()
		{
			base.OnMissionScreenDeactivate();
			this._mpclassloadoutCategory.Unload();
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000694C File Offset: 0x00004B4C
		private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
		{
			if (peer.IsMine && newTeam != null && (newTeam.IsAttacker || newTeam.IsDefender))
			{
				if (this._isActive)
				{
					this.OnTryToggle(false);
				}
				this.OnTryToggle(true);
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000697F File Offset: 0x00004B7F
		private void OnRefreshSelection(MultiplayerClassDivisions.MPHeroClass heroClass)
		{
			this._lastSelectedHeroClass = heroClass;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00006988 File Offset: 0x00004B88
		public override void OnMissionScreenFinalize()
		{
			if (this._gauntletLayer != null)
			{
				base.MissionScreen.RemoveLayer(this._gauntletLayer);
				this._gauntletLayer = null;
			}
			if (this._dataSource != null)
			{
				this._dataSource.OnFinalize();
				this._dataSource = null;
			}
			this._missionLobbyEquipmentNetworkComponent.OnToggleLoadout -= this.OnTryToggle;
			MissionPeer.OnTeamChanged -= this.OnTeamChanged;
			base.OnMissionScreenFinalize();
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00006A00 File Offset: 0x00004C00
		private void CreateView()
		{
			InformationManager.DisplayMessage(new InformationMessage("CrpgMissionGauntletClassLoadout :: CreateView"));
			MissionMultiplayerGameModeBaseClient missionBehaviour = base.Mission.GetMissionBehaviour<MissionMultiplayerGameModeBaseClient>();
			SpriteData spriteData = UIResourceManager.SpriteData;
			TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
			ResourceDepot uiresourceDepot = UIResourceManager.UIResourceDepot;
			this._mpclassloadoutCategory = spriteData.SpriteCategories["ui_mpclassloadout"];
			this._mpclassloadoutCategory.Load(resourceContext, uiresourceDepot);
			this._dataSource = new MultiplayerClassLoadoutVM(missionBehaviour, new Action<MultiplayerClassDivisions.MPHeroClass>(this.OnRefreshSelection), this._lastSelectedHeroClass);
			this._gauntletLayer = new GauntletLayer(this.ViewOrderPriorty, "GauntletLayer");
			this._gauntletLayer.LoadMovie("MultiplayerClassLoadout", this._dataSource);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00006A99 File Offset: 0x00004C99
		private void OnTryToggle(bool isActive)
		{
			if (isActive)
			{
				this._tryToInitialize = true;
				return;
			}
			this.OnToggled(false);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00006AB0 File Offset: 0x00004CB0
		private bool OnToggled(bool isActive)
		{
			if (this._isActive == isActive)
			{
				return true;
			}
			if (!base.MissionScreen.SetDisplayDialog(isActive))
			{
				return false;
			}
			if (isActive)
			{
				this.CreateView();
				this._dataSource.Tick(1f);
				this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
				base.MissionScreen.AddLayer(this._gauntletLayer);
			}
			else
			{
				base.MissionScreen.RemoveLayer(this._gauntletLayer);
				this._dataSource.OnFinalize();
				this._dataSource = null;
				this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
				this._gauntletLayer = null;
			}
			this._isActive = isActive;
			return true;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00006B58 File Offset: 0x00004D58
		public override void OnMissionTick(float dt)
		{
			base.OnMissionTick(dt);
			if (this._tryToInitialize && GameNetwork.IsMyPeerReady && GameNetwork.MyPeer.GetComponent<MissionPeer>().HasSpawnedAgentVisuals && this.OnToggled(true))
			{
				this._tryToInitialize = false;
			}
			if (this._isActive)
			{
				this._dataSource.Tick(dt);
				MissionMultiplayerGameModeFlagDominationClient missionMultiplayerGameModeFlagDominationClient;
				if (base.Input.IsGameKeyPressed(16) && (missionMultiplayerGameModeFlagDominationClient = (base.Mission.GetMissionBehaviour<MissionMultiplayerGameModeBaseClient>() as MissionMultiplayerGameModeFlagDominationClient)) != null)
				{
					missionMultiplayerGameModeFlagDominationClient.OnRequestForfeitSpawn();
				}
			}
		}

		// Token: 0x04000094 RID: 148
		private MultiplayerClassLoadoutVM _dataSource;

		// Token: 0x04000095 RID: 149
		private GauntletLayer _gauntletLayer;

		// Token: 0x04000096 RID: 150
		private SpriteCategory _mpclassloadoutCategory;

		// Token: 0x04000097 RID: 151
		private MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;

		// Token: 0x04000098 RID: 152
		private MissionMultiplayerGameModeBaseClient _gameModeClient;

		// Token: 0x04000099 RID: 153
		private MultiplayerClassDivisions.MPHeroClass _lastSelectedHeroClass;

		// Token: 0x0400009A RID: 154
		private bool _tryToInitialize;

		// Token: 0x0400009B RID: 155
		private bool _isActive;
	}
}
