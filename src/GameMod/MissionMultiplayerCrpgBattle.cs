using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.GameMod
{
    public class MissionMultiplayerCrpgBattle : MissionMultiplayerGameModeBase
    {
      	public override bool IsGameModeHidingAllAgentVisuals
      	{
      		get
      		{
      			return true;
      		}
      	}
        public bool UseGold()
        {
            return true;
        }
        public override bool AllowCustomPlayerBanners()
        {
            return true; // test
        }
        public override bool UseRoundController()
        {
            return true; // test
        }

        public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
      	{
      		return MissionLobbyComponent.MultiplayerGameType.Skirmish;
        }

      	public override void OnBehaviourInitialize()
      	{
      		base.OnBehaviourInitialize();
      		this._missionScoreboardComponent = base.Mission.GetMissionBehaviour<MissionScoreboardComponent>();
      	}

        public override void AfterStart()
      	{
            //++
            this.RoundController.OnRoundStarted += this.OnPreparationStart;
            MissionPeer.OnPreTeamChanged += this.OnPreTeamChanged;
            this.RoundController.OnPreparationEnded += this.OnPreparationEnded;
            //this.WarmupComponent.OnWarmupEnding += this.OnWarmupEnding;
            this.RoundController.OnPreRoundEnding += this.OnRoundEnd;
            this.RoundController.OnPostRoundEnded += this.OnPostRoundEnd;

            string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            string strValue2 = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue);
            BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue2);

            //MultiplayerOptions.OptionType.NumberOfBotsTeam1.SetValue(10, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            //MultiplayerOptions.OptionType.NumberOfBotsTeam2.SetValue(10, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);

            Banner banner = new Banner(@object.BannerKey, @object.BackgroundColor1, @object.ForegroundColor1);
            Banner banner2 = new Banner(object2.BannerKey, object2.BackgroundColor2, object2.ForegroundColor2);
            base.Mission.Teams.Add(BattleSideEnum.Attacker, @object.BackgroundColor1, @object.ForegroundColor1, banner, true, false, true);
            base.Mission.Teams.Add(BattleSideEnum.Defender, object2.BackgroundColor2, object2.ForegroundColor2, banner2, true, false, true);
        }
        protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
        {
            registerer.Register<RequestForfeitSpawn>(new GameNetworkMessage.ClientMessageHandlerDelegate<RequestForfeitSpawn>(this.HandleClientEventRequestForfeitSpawn));
        }
        //++
        public override void OnRemoveBehaviour()
        {
            this.RoundController.OnRoundStarted -= this.OnPreparationStart;
            MissionPeer.OnPreTeamChanged -= this.OnPreTeamChanged;
            this.RoundController.OnPreparationEnded -= this.OnPreparationEnded;
            //this.WarmupComponent.OnWarmupEnding -= this.OnWarmupEnding;
            this.RoundController.OnPreRoundEnding -= this.OnRoundEnd;
            this.RoundController.OnPostRoundEnded -= this.OnPostRoundEnd;
            base.OnRemoveBehaviour();
        }
        public override void OnPeerChangedTeam(NetworkCommunicator peer, Team oldTeam, Team newTeam)
      	{
            if (oldTeam != null && oldTeam != newTeam)
            {
                base.ChangeCurrentGoldForPeer(peer.GetComponent<MissionPeer>(), 0);
            }
        }
        //++
        private void OnPreparationStart()
        {
            this.NotificationsComponent.PreparationStarted();
        }
        //++

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this.MissionLobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing)
            {
                if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
                {
                    this.CheckForPlayersSpawningAsBots();
                }
                /*if (this.RoundController.IsRoundInProgress)
                {
                    if (!this._flagRemovalOccured)
                    {
                        this.CheckRemovingOfPoints();
                    }
                    this.CheckMorales();
                    this.TickFlags(dt);
                }*/
            }
        }
        private void CheckForPlayersSpawningAsBots()
        {
            foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
            {
                if (missionPeer.GetNetworkPeer().IsSynchronized && missionPeer.ControlledAgent == null && missionPeer.Team != null && missionPeer.ControlledFormation != null && missionPeer.SpawnCountThisRound > 0)
                {
                    if (!missionPeer.HasSpawnTimerExpired && missionPeer.SpawnTimer.Check(MBCommon.GetTime(MBCommon.TimeType.Mission)))
                    {
                        missionPeer.HasSpawnTimerExpired = true;
                    }
                    if (missionPeer.HasSpawnTimerExpired && missionPeer.WantsToSpawnAsBot)
                    {
                        List<Agent> list = (from x in missionPeer.ControlledFormation.Units
                                            where x.IsActive() && x.IsAIControlled
                                            select x).ToList<Agent>();
                        if (list.Count > 0)
                        {
                            Agent followedAgent = missionPeer.FollowedAgent;
                            Agent botAgent;
                            if (followedAgent != null && list.Contains(followedAgent))
                            {
                                botAgent = followedAgent;
                            }
                            else
                            {
                                botAgent = list.MaxBy((Agent x) => x.Health);
                            }
                            Mission.Current.ReplaceBotWithPlayer(botAgent, missionPeer);
                            missionPeer.WantsToSpawnAsBot = false;
                            missionPeer.HasSpawnTimerExpired = false;
                        }
                    }
                }
            }
        }
        public float GetTimeUntilBattleSideVictory(BattleSideEnum side)
        {
            float val = float.MaxValue;
            val = this.RoundController.RemainingRoundTime;
            InformationManager.DisplayMessage(new InformationMessage("GetTimeUntilBattleSideVictory :: "+ val));

            return val;
        }
        public override bool CheckForWarmupEnd()
        {
            int[] array = new int[2];
            foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkCommunicator.GetComponent<MissionPeer>();
                if (networkCommunicator.IsSynchronized && ((component != null) ? component.Team : null) != null && component.Team.Side != BattleSideEnum.None)
                {
                    array[(int)component.Team.Side]++;
                }
            }
            return array.Sum() >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        }
        public override bool CheckForRoundEnd()
        {
            /*if (Math.Abs(this._morale) >= 1f)
            {
                return true;
            }*/
            bool flag = base.Mission.AttackerTeam.ActiveAgents.Count > 0;
            bool flag2 = base.Mission.DefenderTeam.ActiveAgents.Count > 0;
            //InformationManager.DisplayMessage(new InformationMessage("CheckForRoundEnd :: " + flag +" - "+ flag2));

            if (flag && flag2)
            {
                return false;
            }
            if (!base.SpawnComponent.AreAgentsSpawning())
            {
                return true;
            }
            bool[] array = new bool[2];
            if (this.UseGold())
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (((component != null) ? component.Team : null) != null && component.Team.Side != BattleSideEnum.None && !array[(int)component.Team.Side])
                    {
                        string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
                        if (component.Team.Side != BattleSideEnum.Attacker)
                        {
                            strValue = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
                        }
                        if (base.GetCurrentGoldForPeer(component) >= MultiplayerClassDivisions.GetMinimumTroopCost(MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue)))
                        {
                            array[(int)component.Team.Side] = true;
                        }
                    }
                }
            }
            return (!flag && !array[1]) || (!flag2 && !array[0]);
        }
        public override bool UseCultureSelection()
        {
            return false;
        }
        private void OnWarmupEnding()
        {
            this.NotificationsComponent.WarmupEnding();
        }
        private void OnRoundEnd()
        {
            bool flag0 = (base.MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.WaitingFirstPlayers);
            InformationManager.DisplayMessage(new InformationMessage("OnRoundEnd :: test"+ flag0));
            CaptureTheFlagCaptureResultEnum captureTheFlagCaptureResultEnum = CaptureTheFlagCaptureResultEnum.NotCaptured;

            bool flag = this.RoundController.RemainingRoundTime <= 0f;
            bool flag2 = base.Mission.AttackerTeam.ActiveAgents.Count <= 0;
            bool flag3 = base.Mission.DefenderTeam.ActiveAgents.Count <= 0;
            if (flag)
            {
                InformationManager.DisplayMessage(new InformationMessage("OnRoundEnd :: Draw"));

                captureTheFlagCaptureResultEnum = CaptureTheFlagCaptureResultEnum.Draw;
                this.RoundController.RoundWinner = BattleSideEnum.None;
                this.RoundController.RoundEndReason = (flag ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded);
            }
            else if (flag2)
            {
                InformationManager.DisplayMessage(new InformationMessage("OnRoundEnd :: DefendersWin"));

                captureTheFlagCaptureResultEnum = CaptureTheFlagCaptureResultEnum.DefendersWin;
                this.RoundController.RoundWinner = BattleSideEnum.Defender;
                this.RoundController.RoundEndReason = RoundEndReason.SideDepleted;

            }
            else if (flag3)
            {
                InformationManager.DisplayMessage(new InformationMessage("OnRoundEnd :: AttackersWin"));

                captureTheFlagCaptureResultEnum = CaptureTheFlagCaptureResultEnum.AttackersWin;
                this.RoundController.RoundWinner = BattleSideEnum.Attacker;
                this.RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (((component != null) ? component.Team : null) != null && component.Team.Side != BattleSideEnum.None)
                    {
                        string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
                        if (component.Team.Side != BattleSideEnum.Attacker)
                        {
                            strValue = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
                        }
                        if (base.GetCurrentGoldForPeer(component) >= MultiplayerClassDivisions.GetMinimumTroopCost(MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue)))
                        {
                            InformationManager.DisplayMessage(new InformationMessage("OnRoundEnd :: autre"));

                            this.RoundController.RoundWinner = component.Team.Side;
                            this.RoundController.RoundEndReason = RoundEndReason.SideDepleted;
                            captureTheFlagCaptureResultEnum = ((component.Team.Side == BattleSideEnum.Attacker) ? CaptureTheFlagCaptureResultEnum.AttackersWin : CaptureTheFlagCaptureResultEnum.DefendersWin);
                            break;
                        }
                    }
                }
            }
            if (captureTheFlagCaptureResultEnum != CaptureTheFlagCaptureResultEnum.NotCaptured)
            {
                this.HandleRoundEnd(captureTheFlagCaptureResultEnum);
            }
        }
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            HealthAgentComponent component = agent.GetComponent<HealthAgentComponent>();
            if (component != null)
            {
                component.UpdateSyncToAllClients(true);
            }
            if (agent.IsPlayerControlled)
            {
                agent.MissionPeer.GetComponent<CrpgBattleMissionRepresentative>().UpdateSelectedClassServer(agent);
            }
        }
        private void HandleRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
        {
            InformationManager.DisplayMessage(new InformationMessage("HandleRoundEnd"));

            AgentVictoryLogic missionBehaviour = base.Mission.GetMissionBehaviour<AgentVictoryLogic>();
            if (missionBehaviour == null)
            {
                return;
            }
            if (roundResult == CaptureTheFlagCaptureResultEnum.AttackersWin)
            {
                missionBehaviour.SetTimersOfVictoryReactions(BattleSideEnum.Attacker);
                return;
            }
            if (roundResult != CaptureTheFlagCaptureResultEnum.DefendersWin)
            {
                return;
            }
            missionBehaviour.SetTimersOfVictoryReactions(BattleSideEnum.Defender);
        }

        // Token: 0x06001FAE RID: 8110 RVA: 0x0006E268 File Offset: 0x0006C468
        private void OnPostRoundEnd()
        {
            InformationManager.DisplayMessage(new InformationMessage("OnPostRoundEnd"));

            if (this.UseGold() && !this.RoundController.IsMatchEnding)
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (component != null && this.RoundController.RoundCount > 0)
                    {
                        int num = 300;
                        /*
                        int num2 = base.GetCurrentGoldForPeer(component);
                        if (num2 < 0)
                        {
                            num2 = 90;
                        }
                        else if (this.RoundController.RoundWinner == component.Team.Side && component.GetComponent<FlagDominationMissionRepresentative>().CheckIfSurvivedLastRoundAndReset())
                        {
                            num2 += 30;
                        }
                        num += MBMath.ClampInt(num2, 0, 90);
                        if (num > 300)
                        {
                            int carriedGoldAmount = num - 300;
                            this.NotificationsComponent.GoldCarriedFromPreviousRound(carriedGoldAmount, component.GetNetworkPeer());
                        }*/
                        base.ChangeCurrentGoldForPeer(component, num);
                    }
                }
            }
        }
        protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
        {
            if (this.RoundController.IsRoundInProgress && MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
            {
                this.MakePlayerFormationCharge(networkPeer);
            }
        }
        // Token: 0x06001FB0 RID: 8112 RVA: 0x0006E381 File Offset: 0x0006C581
        private void OnPreTeamChanged(NetworkCommunicator peer, Team currentTeam, Team newTeam)
        {
            if (peer.IsSynchronized && peer.GetComponent<MissionPeer>().ControlledAgent != null)
            {
                this.MakePlayerFormationCharge(peer);
            }
        }

        // Token: 0x06001FB1 RID: 8113 RVA: 0x0006E3A0 File Offset: 0x0006C5A0
        private void OnPreparationEnded()
        {
            InformationManager.DisplayMessage(new InformationMessage("OnPreparationEnded"));

            if (this.UseGold())
            {
                List<MissionPeer>[] array = new List<MissionPeer>[2];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new List<MissionPeer>();
                }
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (component != null && component.Team != null && component.Team.Side != BattleSideEnum.None)
                    {
                        array[(int)component.Team.Side].Add(component);
                    }
                }
                int num = array[1].Count - array[0].Count;
                BattleSideEnum battleSideEnum = (num == 0) ? BattleSideEnum.None : ((num < 0) ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
                if (battleSideEnum != BattleSideEnum.None)
                {
                    num = Math.Abs(num);
                    int count = array[(int)battleSideEnum].Count;
                    if (count > 0)
                    {
                        int num2 = 300 * num / 10 / count * 10;
                        foreach (MissionPeer peer in array[(int)battleSideEnum])
                        {
                            InformationManager.DisplayMessage(new InformationMessage("OnPreparationEnded" + num2));
                            base.ChangeCurrentGoldForPeer(peer, base.GetCurrentGoldForPeer(peer) + num2);
                        }
                    }
                }
            }
        }

        // Token: 0x06001FB2 RID: 8114 RVA: 0x0006E4E4 File Offset: 0x0006C6E4
        private void MakePlayerFormationCharge(NetworkCommunicator peer)
        {
            if (peer.IsSynchronized)
            {
                MissionPeer component = peer.GetComponent<MissionPeer>();
                if (component.ControlledFormation != null && component.ControlledAgent != null)
                {
                    component.Team.GetOrderControllerOf(component.ControlledAgent).SetOrder(OrderType.Charge);
                }
            }
        }
        protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            InformationManager.DisplayMessage(new InformationMessage("HandleNewClientAfterSynchronized"));
            networkPeer.AddComponent<CrpgBattleMissionRepresentative>();
            if (this.UseGold() && !this.RoundController.IsRoundInProgress)
            {
                InformationManager.DisplayMessage(new InformationMessage("HandleNewClientAfterSynchronized :: not IsRoundInProgress"));
                base.ChangeCurrentGoldForPeer(networkPeer.GetComponent<MissionPeer>(), 300);
                MissionMultiplayerCrpgBattleClient crpgBattleClient = this._crpgBattleClient;
                if (crpgBattleClient != null)
                {
                    crpgBattleClient.OnGoldAmountChangedForRepresentative(networkPeer.GetComponent<CrpgBattleMissionRepresentative>(), 300);
                }
                //this.GameModeBaseClient.OnGoldAmountChangedForRepresentative(networkPeer.GetComponent<CrpgBattleMissionRepresentative>(), 240);
            }

        }
        // Token: 0x06001FB4 RID: 8116 RVA: 0x0006E614 File Offset: 0x0006C814
        private bool HandleClientEventRequestForfeitSpawn(NetworkCommunicator peer, RequestForfeitSpawn message)
        {
            this.ForfeitSpawning(peer);
            return true;
        }

        // Token: 0x06001FB5 RID: 8117 RVA: 0x0006E620 File Offset: 0x0006C820
        public void ForfeitSpawning(NetworkCommunicator peer)
        {
            InformationManager.DisplayMessage(new InformationMessage("ForfeitSpawning"));
            MissionPeer component = peer.GetComponent<MissionPeer>();
            if (component != null && component.HasSpawnedAgentVisuals && this.UseGold() && this.RoundController.IsRoundInProgress)
            {
                Mission.Current.GetMissionBehaviour<MultiplayerMissionAgentVisualSpawnComponent>().RemoveAgentVisuals(component, true);
                base.ChangeCurrentGoldForPeer(component, -1);
            }
        }

        // Token: 0x06001FB6 RID: 8118 RVA: 0x0006E670 File Offset: 0x0006C870
        public static void SetWinnerTeam(int winnerTeamNo)
        {
            InformationManager.DisplayMessage(new InformationMessage("SetWinnerTeam"));

            Mission mission = Mission.Current;
            MissionMultiplayerCrpgBattle missionBehaviour = mission.GetMissionBehaviour<MissionMultiplayerCrpgBattle>();
            if (missionBehaviour != null)
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    missionBehaviour.ChangeCurrentGoldForPeer(component, 0);
                }
                for (int i = mission.Agents.Count - 1; i >= 0; i--)
                {
                    Agent agent = mission.Agents[i];
                    if (agent.IsHuman && agent.Team.MBTeam.Index != winnerTeamNo + 1)
                    {
                        Mission.Current.KillAgentCheat(agent);
                    }
                }
            }
        }

        // Token: 0x06001FB7 RID: 8119 RVA: 0x0006E72C File Offset: 0x0006C92C
        protected override void OnEndMission()
        {
            InformationManager.DisplayMessage(new InformationMessage("OnEndMission"));
            if (this.UseGold())
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (component != null)
                    {
                        base.ChangeCurrentGoldForPeer(component, -1);
                    }
                }
            }
        }
        /*public override int GetScoreForKill(Agent killedAgent)
      	{
            return MultiplayerClassDivisions.GetMPHeroClassForCharacter(killedAgent.Character).TroopCost;
            //return 0;
        }
      
      	public override int GetScoreForAssist(Agent killedAgent)
      	{
            return (int)((float)MultiplayerClassDivisions.GetMPHeroClassForCharacter(killedAgent.Character).TroopCost * 0.5f);
            //return 0;
        }*/
        
          public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
          {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (affectedAgent.IsPlayerControlled)
            {
                affectedAgent.MissionPeer.GetComponent<CrpgBattleMissionRepresentative>().UpdateSelectedClassServer(null);
            }
            /*if (blow.DamageType != DamageTypes.Invalid && (agentState == AgentState.Unconscious || agentState == AgentState.Killed) && affectedAgent.IsHuman)
              {
                  if (affectorAgent != null && affectorAgent.IsEnemyOf(affectedAgent))
                  {
                      this._missionScoreboardComponent.ChangeTeamScore(affectorAgent.Team, this.GetScoreForKill(affectedAgent));
                  }
                  else
                  {
                      this._missionScoreboardComponent.ChangeTeamScore(affectedAgent.Team, -this.GetScoreForKill(affectedAgent));
                  }
                  MissionPeer missionPeer = affectedAgent.MissionPeer;
                  if (missionPeer != null)
                  {
                      int num = 100;
                      if (affectorAgent != affectedAgent)
                      {
                          List<MissionPeer>[] array = new List<MissionPeer>[2];
                          for (int i = 0; i < array.Length; i++)
                          {
                              array[i] = new List<MissionPeer>();
                          }
                          foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                          {
                              MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                              if (component != null && component.Team != null && component.Team.Side != BattleSideEnum.None)
                              {
                                  array[(int)component.Team.Side].Add(component);
                              }
                          }
                          int num2 = array[1].Count - array[0].Count;
                          BattleSideEnum battleSideEnum = (num2 == 0) ? BattleSideEnum.None : ((num2 < 0) ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
                          if (battleSideEnum != BattleSideEnum.None && battleSideEnum == missionPeer.Team.Side)
                          {
                              num2 = Math.Abs(num2);
                              int count = array[(int)battleSideEnum].Count;
                              if (count > 0)
                              {
                                  int num3 = num * num2 / 10 / count * 10;
                                  num += num3;
                              }
                          }
                      }
                      base.ChangeCurrentGoldForPeer(missionPeer, missionPeer.Representative.Gold + num);
                  }
                  MultiplayerClassDivisions.MPHeroClass mpheroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(affectedAgent.Character);
                  Agent affectorAgent2 = affectorAgent;
                  if (((affectorAgent2 != null) ? affectorAgent2.MissionPeer : null) != null && affectorAgent != affectedAgent && !affectorAgent.IsFriendOf(affectedAgent))
                  {
                      CrpgBattleMissionRepresentative CrpgBattleMissionRepresentative = affectorAgent.MissionPeer.Representative as CrpgBattleMissionRepresentative;
                      int goldGainsFromKillDataAndUpdateFlags = CrpgBattleMissionRepresentative.GetGoldGainsFromKillDataAndUpdateFlags(mpheroClassForCharacter, false, blow.MissileRecordIsValid);
                      base.ChangeCurrentGoldForPeer(affectorAgent.MissionPeer, CrpgBattleMissionRepresentative.Gold + goldGainsFromKillDataAndUpdateFlags);
                  }
                  List<Agent.Hitter> list = (from hitter in affectedAgent.HitterList
                                              where hitter.HitterPeer != affectorAgent.MissionPeer
                                              select hitter).ToList<Agent.Hitter>();
                  if (list.Count > 0)
                  {
                      Agent.Hitter hitter2 = list.MaxBy((Agent.Hitter hitter) => hitter.Damage);
                      if (hitter2.Damage >= 35f && !hitter2.IsFriendlyHit)
                      {
                          CrpgBattleMissionRepresentative CrpgBattleMissionRepresentative2 = hitter2.HitterPeer.Representative as CrpgBattleMissionRepresentative;
                          int goldGainsFromKillDataAndUpdateFlags2 = CrpgBattleMissionRepresentative2.GetGoldGainsFromKillDataAndUpdateFlags(mpheroClassForCharacter, true, blow.MissileRecordIsValid);
                          base.ChangeCurrentGoldForPeer(hitter2.HitterPeer, CrpgBattleMissionRepresentative2.Gold + goldGainsFromKillDataAndUpdateFlags2);
                      }
                  }
              }*/
          }
        public override float GetTroopNumberMultiplierForMissingPlayer(MissionPeer spawningPeer)
        {
            return 1f;
        }

        // Token: 0x06001FBF RID: 8127 RVA: 0x0006EBE8 File Offset: 0x0006CDE8
        protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
        }
        public override bool CheckForMatchEnd()
      	{
            bool ok = false;
            int num = 0;
            foreach (Agent agent in base.Mission.AttackerTeam.ActiveAgents)
            {
                if (agent.Character != null && agent.MissionPeer == null)
                {
                    num++;
                    ok = true; // a simplifier pas besoin de tt la boucle
                }
            }
            int num2 = 0;
            foreach (Agent agent2 in base.Mission.DefenderTeam.ActiveAgents)
            {
                if (agent2.Character != null && agent2.MissionPeer == null)
                {
                    num2++;
                    ok = true; // a simplifier pas besoin de tt la boucle
                }
            }
            InformationManager.DisplayMessage(new InformationMessage("CheckForMatchEnd :: " + ok));

            return ok;
            //int minScoreToWinMatch = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            //return this._missionScoreboardComponent.Sides.Any((MissionScoreboardComponent.MissionScoreboardSide side) => side.SideScore >= minScoreToWinMatch);
        }

        public override Team GetWinnerTeam()
      	{
            InformationManager.DisplayMessage(new InformationMessage("GetWinnerTeam"));

            int intValue = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            Team result = null;
            MissionScoreboardComponent.MissionScoreboardSide[] sides = this._missionScoreboardComponent.Sides;
            if (sides[1].SideScore < intValue && sides[0].SideScore >= intValue)
            {
                result = base.Mission.Teams.Defender;
            }
            if (sides[0].SideScore < intValue && sides[1].SideScore >= intValue)
            {
                result = base.Mission.Teams.Attacker;
            }
            return result;
        }

        // Token: 0x04000BDA RID: 3034
        // public const int MaxScoreToEndMatch = 120000;

        // Token: 0x04000BDB RID: 3035
        private const int FirstSpawnGold = 120;

        // Token: 0x04000BDC RID: 3036
        private const int RespawnGold = 0;

        // Token: 0x04000BDD RID: 3037
        private MissionScoreboardComponent _missionScoreboardComponent;
        // Token: 0x04000BAF RID: 2991
        private MissionMultiplayerCrpgBattleClient _crpgBattleClient;
    }
}