using System;
using System.Collections.Generic;
using NetworkMessages.FromServer;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod
{
    public class CrpgBattleMissionRepresentative: MissionRepresentativeBase

    {
		public override bool IsThereAgentAction(Agent targetAgent)
		{
			return false;
		}
		public override void OnAgentInteraction(Agent targetAgent)
		{
		}
		public int GetGoldAmountForVisual()
		{
			if (base.Gold < 0)
			{
				return 0;
			}
			return base.Gold;
		}
		public void UpdateSelectedClassServer(Agent agent)
		{
			this._survivedLastRound = (agent != null);
		}
		public bool CheckIfSurvivedLastRoundAndReset()
		{
			bool survivedLastRound = this._survivedLastRound;
			this._survivedLastRound = false;
			return survivedLastRound;
		}
		/*public override void OnAgentSpawned()
		{
			//this._currentGoldGains = (GoldGainFlags)0;
			this._killCountOnSpawn = base.MissionPeer.KillCount;
			this._assistCountOnSpawn = base.MissionPeer.AssistCount;
		}*/

		// Token: 0x04001289 RID: 4745
		private bool _survivedLastRound;
	


		/*public int GetGoldGainsFromKillDataAndUpdateFlags(MultiplayerClassDivisions.MPHeroClass victimClass, bool isAssist, bool isRanged)
		{
			int num = 0;
			List<KeyValuePair<ushort, int>> list = new List<KeyValuePair<ushort, int>>();
			if (isAssist)
			{
				int num2 = 1;
				switch (base.MissionPeer.AssistCount - this._assistCountOnSpawn)
				{
					case 1:
						num += 10;
						this._currentGoldGains |= GoldGainFlags.FirstAssist;
						list.Add(new KeyValuePair<ushort, int>(4, 10));
						break;
					case 2:
						num += 10;
						this._currentGoldGains |= GoldGainFlags.SecondAssist;
						list.Add(new KeyValuePair<ushort, int>(8, 10));
						break;
					case 3:
						num += 10;
						this._currentGoldGains |= GoldGainFlags.ThirdAssist;
						list.Add(new KeyValuePair<ushort, int>(16, 10));
						break;
					default:
						num += num2;
						list.Add(new KeyValuePair<ushort, int>(256, num2));
						break;
				}
			}
			else
			{
				if (base.ControlledAgent != null)
				{
					int num3 = victimClass.TroopCost - MultiplayerClassDivisions.GetMPHeroClassForCharacter(base.ControlledAgent.Character).TroopCost;
					int num4 = 2 + Math.Max(0, num3 / 2);
					num += num4;
					list.Add(new KeyValuePair<ushort, int>(128, num4));
				}
				int num5 = base.MissionPeer.KillCount - this._killCountOnSpawn;
				if (num5 != 5)
				{
					if (num5 == 10)
					{
						num += 30;
						this._currentGoldGains |= GoldGainFlags.TenthKill;
						list.Add(new KeyValuePair<ushort, int>(64, 30));
					}
				}
				else
				{
					num += 20;
					this._currentGoldGains |= GoldGainFlags.FifthKill;
					list.Add(new KeyValuePair<ushort, int>(32, 20));
				}
				if (isRanged && !this._currentGoldGains.HasAnyFlag(GoldGainFlags.FirstRangedKill))
				{
					num += 10;
					this._currentGoldGains |= GoldGainFlags.FirstRangedKill;
					list.Add(new KeyValuePair<ushort, int>(1, 10));
				}
				if (!isRanged && !this._currentGoldGains.HasAnyFlag(GoldGainFlags.FirstMeleeKill))
				{
					num += 10;
					this._currentGoldGains |= GoldGainFlags.FirstMeleeKill;
					list.Add(new KeyValuePair<ushort, int>(2, 10));
				}
			}
			int num6 = 0;
			if (base.MissionPeer.Team == Mission.Current.Teams.Attacker)
			{
				num6 = MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			}
			else if (base.MissionPeer.Team == Mission.Current.Teams.Defender)
			{
				num6 = MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			}
			if (num6 != 0 && (num > 0 || list.Count > 0))
			{
				float num7 = 1f + (float)num6 * 0.01f;
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = new KeyValuePair<ushort, int>(list[i].Key, list[i].Value + (int)((float)list[i].Value * num7));
				}
				num += (int)((float)num * num7);
			}
			if (list.Count > 0 && !base.Peer.Communicator.IsServerPeer && base.Peer.Communicator.IsConnectionActive)
			{
				GameNetwork.BeginModuleEventAsServer(base.Peer);
				GameNetwork.WriteMessage(new TDMGoldGain(list));
				GameNetwork.EndModuleEventAsServer();
			}
			return num;
		}

		// Token: 0x04001294 RID: 4756
		private const int FirstRangedKillGold = 10;

		// Token: 0x04001295 RID: 4757
		private const int FirstMeleeKillGold = 10;

		// Token: 0x04001296 RID: 4758
		private const int FirstAssistGold = 10;

		// Token: 0x04001297 RID: 4759
		private const int SecondAssistGold = 10;

		// Token: 0x04001298 RID: 4760
		private const int ThirdAssistGold = 10;

		// Token: 0x04001299 RID: 4761
		private const int FifthKillGold = 20;

		// Token: 0x0400129A RID: 4762
		private const int TenthKillGold = 30;

		// Token: 0x0400129B RID: 4763
		private GoldGainFlags _currentGoldGains;*/

		// Token: 0x0400129C RID: 4764
		//private int _killCountOnSpawn;

		// Token: 0x0400129D RID: 4765
		//private int _assistCountOnSpawn;
	}
}
