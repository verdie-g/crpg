using System;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
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
				return MissionLobbyComponent.MultiplayerGameType.TeamDeathmatch;
			}
		}

	
		public override void OnBehaviourInitialize()
		{
			base.OnBehaviourInitialize();
			NetworkCommunicator.OnPeerComponentAdded += this.OnPeerComponentAdded;
		}

		public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
		{
			/*if (representative != null && base.MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Ending)
			{
				representative.UpdateGold(goldAmount);
				base.ScoreboardComponent.PlayerPropertiesChanged(representative.MissionPeer);
			}*/
		}


		/*public override void AfterStart()
		{
			base.Mission.SetMissionMode(MissionMode.Battle, true);
		}*/

		protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
		{
			if (GameNetwork.IsClient)
			{
				//registerer.Register<SyncGoldsForSkirmish>(new GameNetworkMessage.ServerMessageHandlerDelegate<SyncGoldsForSkirmish>(this.HandleServerEventUpdateGold));
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



		private void OnPeerComponentAdded(PeerComponent component)
		{
			if (component.IsMine && component is MissionRepresentativeBase)
			{
				this._myRepresentative = (component as CrpgBattleMissionRepresentative);
			}
		}

		private void HandleServerEventUpdateGold(SyncGoldsForSkirmish message)
		{
			MissionRepresentativeBase component = message.VirtualPlayer.GetComponent<MissionRepresentativeBase>();
			this.OnGoldAmountChangedForRepresentative(component, message.GoldAmount);
		}

		private void HandleServerEventTDMGoldGain(TDMGoldGain message)
		{
			Action<TDMGoldGain> onGoldGainEvent = this.OnGoldGainEvent;
			if (onGoldGainEvent == null)
			{
				return;
			}
			onGoldGainEvent(message);
		}
		public override int GetGoldAmount()
		{
			//return this._myRepresentative.Gold;
			return 0;
		}



		public override void OnRemoveBehaviour()
		{
			NetworkCommunicator.OnPeerComponentAdded -= this.OnPeerComponentAdded;
		}

		private CrpgBattleMissionRepresentative _myRepresentative;
	}
}
