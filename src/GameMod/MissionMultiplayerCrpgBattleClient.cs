using System;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.GameMod
{
    public class MissionMultiplayerCrpgBattleClient : MissionMultiplayerGameModeBaseClient
    {
		public event Action<TDMGoldGain> OnGoldGainEvent;

		public override bool IsGameModeUsingGold
		{
			get
			{
				return true;
			}
		}

		public override bool IsGameModeTactical
		{
			get
			{
				return false;
			}
		}

		public override bool IsGameModeUsingRoundCountdown
		{
			get
			{
				return true;
			}
		}

		public override MissionLobbyComponent.MultiplayerGameType GameType
		{
			get
			{
				return MissionLobbyComponent.MultiplayerGameType.Skirmish;
			}
		}

	
		public override void OnBehaviourInitialize()
		{
			base.OnBehaviourInitialize();
			this._scoreboardComponent = Mission.Current.GetMissionBehaviour<MissionScoreboardComponent>();
			NetworkCommunicator.OnPeerComponentAdded += this.OnPeerComponentAdded;
		}

		public override void OnRemoveBehaviour()
		{
			NetworkCommunicator.OnPeerComponentAdded -= this.OnPeerComponentAdded;
		}
		private void OnPeerComponentAdded(PeerComponent component)
		{
			if (component.IsMine && component is MissionRepresentativeBase)
			{
				this._myRepresentative = (component as CrpgBattleMissionRepresentative);
			}
		}
		public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
		{

			MissionPeer component = representative.GetComponent<MissionPeer>();
			representative.UpdateGold(goldAmount);
			this._scoreboardComponent.PlayerPropertiesChanged(component);

			/*if (representative != null && base.MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Ending)
			{
				representative.UpdateGold(goldAmount);
				base.ScoreboardComponent.PlayerPropertiesChanged(representative.MissionPeer);
			}*/


			
		}
		public override void AfterStart()
		{
			base.Mission.SetMissionMode(MissionMode.Battle, true);
			Game.Current.GameType.CurrentGame.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/cRPG/ModuleData/native_strings.xml");

		}

		protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
		{
			if (GameNetwork.IsClient)
			{
				registerer.Register<SyncGoldsForSkirmish>(new GameNetworkMessage.ServerMessageHandlerDelegate<SyncGoldsForSkirmish>(this.HandleServerEventUpdateGold));
				//registerer.Register<TDMGoldGain>(new GameNetworkMessage.ServerMessageHandlerDelegate<TDMGoldGain>(this.HandleServerEventTDMGoldGain));
				//++
				registerer.Register<BotsControlledChange>(new GameNetworkMessage.ServerMessageHandlerDelegate<BotsControlledChange>(this.HandleServerEventBotsControlledChangeEvent));

			}
		}

		private void HandleServerEventBotsControlledChangeEvent(BotsControlledChange message)
		{
			MissionPeer component = message.Peer.GetComponent<MissionPeer>();
			this.OnBotsControlledChanged(component, message.AliveCount, message.TotalCount);
		}
		public void OnBotsControlledChanged(MissionPeer missionPeer, int botAliveCount, int botTotalCount)
		{
			//missionPeer.BotsUnderControlAlive = botAliveCount;
			//missionPeer.BotsUnderControlTotal = botTotalCount;
		}
		public void OnRequestForfeitSpawn()
		{
			if (GameNetwork.IsClient)
			{
				GameNetwork.BeginModuleEventAsClient();
				GameNetwork.WriteMessage(new RequestForfeitSpawn());
				GameNetwork.EndModuleEventAsClient();
				return;
			}
			Mission.Current.GetMissionBehaviour<MissionMultiplayerCrpgBattle>().ForfeitSpawning(GameNetwork.MyPeer);
		}
		private void HandleServerEventUpdateGold(SyncGoldsForSkirmish message)
		{
			MissionRepresentativeBase component = message.VirtualPlayer.GetComponent<MissionRepresentativeBase>();
			this.OnGoldAmountChangedForRepresentative(component, message.GoldAmount);
			InformationManager.DisplayMessage(new InformationMessage("HandleServerEventUpdateGold"+ message.GoldAmount));
		}

		/*private void HandleServerEventTDMGoldGain(TDMGoldGain message)
		{
			InformationManager.DisplayMessage(new InformationMessage("HandleServerEventTDMGoldGain"));
			Action<TDMGoldGain> onGoldGainEvent = this.OnGoldGainEvent;
			if (onGoldGainEvent == null)
			{
				return;
			}
			onGoldGainEvent(message);
		}*/
		public override int GetGoldAmount()
		{
			//InformationManager.DisplayMessage(new InformationMessage("GetGoldAmount"));
			return this._myRepresentative.Gold;
			//return 0;
		}

		private CrpgBattleMissionRepresentative _myRepresentative;

		private MissionScoreboardComponent _scoreboardComponent;
	}
}
