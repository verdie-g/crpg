using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.GameMod
{
    public class MissionMultiplayerBattle :  MissionMultiplayerGameModeBase
    {      
      	public override bool IsGameModeHidingAllAgentVisuals
      	{
      		get
      		{
      			return true;
      		}
      	}
      
      	public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
      	{
      		return MissionLobbyComponent.MultiplayerGameType.Battle;
      	}
      
      	public override void OnBehaviourInitialize()
      	{
      		base.OnBehaviourInitialize();
      		this._missionScoreboardComponent = base.Mission.GetMissionBehaviour<MissionScoreboardComponent>();
      	}

            public override void AfterStart()
      	{

        string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        string strValue2 = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue);
        BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue2);
            
        //++
        MultiplayerOptions.OptionType.NumberOfBotsTeam1.SetValue(10, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        MultiplayerOptions.OptionType.NumberOfBotsTeam2.SetValue(10, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);

        Banner banner = new Banner(@object.BannerKey, @object.BackgroundColor1, @object.ForegroundColor1);
        Banner banner2 = new Banner(object2.BannerKey, object2.BackgroundColor2, object2.ForegroundColor2);
        base.Mission.Teams.Add(BattleSideEnum.Attacker, @object.BackgroundColor1, @object.ForegroundColor1, banner, true, false, true);
        base.Mission.Teams.Add(BattleSideEnum.Defender, object2.BackgroundColor2, object2.ForegroundColor2, banner2, true, false, true);
    }
      
      	protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
      	{
      		networkPeer.AddComponent<BattleMissionRepresentative>();
            //base.ChangeCurrentGoldForPeer(networkPeer.GetComponent<MissionPeer>(), 120);
            //this.GameModeBaseClient.OnGoldAmountChangedForRepresentative(networkPeer.GetComponent<BattleMissionRepresentative>(), 120);

        }

        public override void OnPeerChangedTeam(NetworkCommunicator peer, Team oldTeam, Team newTeam)
      	{
            /*if (oldTeam != null && oldTeam != newTeam)
            {
                base.ChangeCurrentGoldForPeer(peer.GetComponent<MissionPeer>(), 100);
            }*/
        }
      
      	public override int GetScoreForKill(Agent killedAgent)
      	{
      		return MultiplayerClassDivisions.GetMPHeroClassForCharacter(killedAgent.Character).TroopCost;
      	}
      
      	public override int GetScoreForAssist(Agent killedAgent)
      	{
      		return (int)((float)MultiplayerClassDivisions.GetMPHeroClassForCharacter(killedAgent.Character).TroopCost * 0.5f);
      	}
      
      	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
      	{
            if (blow.DamageType != DamageTypes.Invalid && (agentState == AgentState.Unconscious || agentState == AgentState.Killed) && affectedAgent.IsHuman)
            {
                if (affectorAgent != null && affectorAgent.IsEnemyOf(affectedAgent))
                {
                    this._missionScoreboardComponent.ChangeTeamScore(affectorAgent.Team, this.GetScoreForKill(affectedAgent));
                }
                else
                {
                    this._missionScoreboardComponent.ChangeTeamScore(affectedAgent.Team, -this.GetScoreForKill(affectedAgent));
                }
                /*MissionPeer missionPeer = affectedAgent.MissionPeer;
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
                    BattleMissionRepresentative BattleMissionRepresentative = affectorAgent.MissionPeer.Representative as BattleMissionRepresentative;
                    int goldGainsFromKillDataAndUpdateFlags = BattleMissionRepresentative.GetGoldGainsFromKillDataAndUpdateFlags(mpheroClassForCharacter, false, blow.MissileRecordIsValid);
                    base.ChangeCurrentGoldForPeer(affectorAgent.MissionPeer, BattleMissionRepresentative.Gold + goldGainsFromKillDataAndUpdateFlags);
                }
                List<Agent.Hitter> list = (from hitter in affectedAgent.HitterList
                                            where hitter.HitterPeer != affectorAgent.MissionPeer
                                            select hitter).ToList<Agent.Hitter>();
                if (list.Count > 0)
                {
                    Agent.Hitter hitter2 = list.MaxBy((Agent.Hitter hitter) => hitter.Damage);
                    if (hitter2.Damage >= 35f && !hitter2.IsFriendlyHit)
                    {
                        BattleMissionRepresentative BattleMissionRepresentative2 = hitter2.HitterPeer.Representative as BattleMissionRepresentative;
                        int goldGainsFromKillDataAndUpdateFlags2 = BattleMissionRepresentative2.GetGoldGainsFromKillDataAndUpdateFlags(mpheroClassForCharacter, true, blow.MissileRecordIsValid);
                        base.ChangeCurrentGoldForPeer(hitter2.HitterPeer, BattleMissionRepresentative2.Gold + goldGainsFromKillDataAndUpdateFlags2);
                    }
                }*/
            }
        }
      
      	public override bool CheckForMatchEnd()
      	{
            int minScoreToWinMatch = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            return this._missionScoreboardComponent.Sides.Any((MissionScoreboardComponent.MissionScoreboardSide side) => side.SideScore >= minScoreToWinMatch);
        }

        public override Team GetWinnerTeam()
      	{
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
        public const int MaxScoreToEndMatch = 120000;

        // Token: 0x04000BDB RID: 3035
        private const int FirstSpawnGold = 120;

        // Token: 0x04000BDC RID: 3036
        private const int RespawnGold = 100;

        // Token: 0x04000BDD RID: 3037
        private MissionScoreboardComponent _missionScoreboardComponent;
    }
}