/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade
{
	// Token: 0x020002C9 RID: 713
	public class MultiplayerOptions
	{
		// Token: 0x17000680 RID: 1664
		// (get) Token: 0x06002338 RID: 9016 RVA: 0x0007F9E7 File Offset: 0x0007DBE7
		public static MultiplayerOptions Instance
		{
			get
			{
				MultiplayerOptions result;
				if ((result = MultiplayerOptions._instance) == null)
				{
					result = (MultiplayerOptions._instance = new MultiplayerOptions());
				}
				return result;
			}
		}

		// Token: 0x06002339 RID: 9017 RVA: 0x0007FA00 File Offset: 0x0007DC00
		public MultiplayerOptions()
		{
			this._current = new MultiplayerOptions.MultiplayerOptionsContainer();
			this._next = new MultiplayerOptions.MultiplayerOptionsContainer();
			this._ui = new MultiplayerOptions.MultiplayerOptionsContainer();
			MultiplayerOptions.MultiplayerOptionsContainer container = this.GetContainer(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			for (MultiplayerOptions.OptionType optionType = MultiplayerOptions.OptionType.ServerName; optionType < MultiplayerOptions.OptionType.NumOfSlots; optionType++)
			{
				container.CreateOption(optionType);
			}
			List<MultiplayerGameTypeInfo> multiplayerGameTypes = Module.CurrentModule.GetMultiplayerGameTypes();
			if (multiplayerGameTypes.Count > 0)
			{
				MultiplayerGameTypeInfo multiplayerGameTypeInfo = multiplayerGameTypes[0];
				container.UpdateOptionValue(MultiplayerOptions.OptionType.GameType, multiplayerGameTypeInfo.GameType);
				container.UpdateOptionValue(MultiplayerOptions.OptionType.Map, multiplayerGameTypeInfo.Scenes.First<string>());
			}
			container.UpdateOptionValue(MultiplayerOptions.OptionType.CultureTeam1, MBObjectManager.Instance.GetObjectTypeList<BasicCultureObject>()[0].StringId);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.CultureTeam2, MBObjectManager.Instance.GetObjectTypeList<BasicCultureObject>()[2].StringId);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.MaxNumberOfPlayers, 120);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart, 1);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.WarmupTimeLimit, 5);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.MapTimeLimit, 30);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.RoundTimeLimit, 120);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.RoundPreparationTimeLimit, 10);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.RoundTotal, 20);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.RespawnPeriodTeam1, 3);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.RespawnPeriodTeam2, 3);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.MinScoreToWinMatch, 120000);
			container.UpdateOptionValue(MultiplayerOptions.OptionType.AutoTeamBalanceThreshold, 0);
			this._current.CopyAllValuesTo(this._next);
			this._current.CopyAllValuesTo(this._ui);
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x0007FB55 File Offset: 0x0007DD55
		public MultiplayerOptions.MultiplayerOption GetOptionFromOptionType(MultiplayerOptions.OptionType optionType, MultiplayerOptions.MultiplayerOptionsAccessMode mode = MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)
		{
			return this.GetContainer(mode).GetOptionFromOptionType(optionType);
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x0007FB64 File Offset: 0x0007DD64
		public void OnGameTypeChanged(MultiplayerOptions.MultiplayerOptionsAccessMode mode = MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)
		{
			string strValue = MultiplayerOptions.OptionType.GameType.GetStrValue(mode);
			if (!(strValue == "FreeForAll"))
			{
				if (!(strValue == "TeamDeathmatch"))
				{
					if (!(strValue == "Duel"))
					{
						if (!(strValue == "Siege"))
						{
							if (!(strValue == "Captain"))
							{
								if (strValue == "Skirmish")
								{
									this.InitializeForSkirmish(mode);
								}
							}
							else
							{
								this.InitializeForCaptain(mode);
							}
						}
						else
						{
							this.InitializeForSiege(mode);
						}
					}
					else
					{
						this.InitializeForDuel(mode);
					}
				}
				else
				{
					this.InitializeForTeamDeathmatch(mode);
				}
			}
			else
			{
				this.InitializeForFreeForAll(mode);
			}
			MultiplayerOptions.OptionType.Map.SetValue(this.GetMapList()[0], MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x0007FC14 File Offset: 0x0007DE14
		private void InitializeForFreeForAll(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(120, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(0, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(30, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.MinScoreToWinMatch.SetValue(120000, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(0, mode);
		}

		// Token: 0x0600233D RID: 9021 RVA: 0x0007FCA8 File Offset: 0x0007DEA8
		private void InitializeForTeamDeathmatch(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(120, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(0, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(30, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.MinScoreToWinMatch.SetValue(120000, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(1, mode);
		}

		// Token: 0x0600233E RID: 9022 RVA: 0x0007FD3C File Offset: 0x0007DF3C
		private void InitializeForDuel(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(120, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(0, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(MultiplayerOptions.OptionType.MapTimeLimit.GetMaximumValue(), mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(0, mode);
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x0007FDC8 File Offset: 0x0007DFC8
		private void InitializeForSiege(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(120, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(0, mode);
			MultiplayerOptions.OptionType.WarmupTimeLimit.SetValue(3, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(30, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(12, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(30, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(1, mode);
		}

		// Token: 0x06002340 RID: 9024 RVA: 0x0007FE5C File Offset: 0x0007E05C
		private void InitializeForCaptain(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(12, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(25, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(6, mode);
			MultiplayerOptions.OptionType.WarmupTimeLimit.SetValue(5, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(5, mode);
			MultiplayerOptions.OptionType.RoundTimeLimit.SetValue(600, mode);
			MultiplayerOptions.OptionType.RoundPreparationTimeLimit.SetValue(20, mode);
			MultiplayerOptions.OptionType.RoundTotal.SetValue(5, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(1, mode);
		}

		// Token: 0x06002341 RID: 9025 RVA: 0x0007FF0C File Offset: 0x0007E10C
		private void InitializeForSkirmish(MultiplayerOptions.MultiplayerOptionsAccessMode mode)
		{
			MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(12, mode);
			MultiplayerOptions.OptionType.NumberOfBotsPerFormation.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.SetValue(0, mode);
			MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.SetValue(50, mode);
			MultiplayerOptions.OptionType.SpectatorCamera.SetValue(6, mode);
			MultiplayerOptions.OptionType.WarmupTimeLimit.SetValue(5, mode);
			MultiplayerOptions.OptionType.MapTimeLimit.SetValue(5, mode);
			MultiplayerOptions.OptionType.RoundTimeLimit.SetValue(420, mode);
			MultiplayerOptions.OptionType.RoundPreparationTimeLimit.SetValue(20, mode);
			MultiplayerOptions.OptionType.RoundTotal.SetValue(5, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam1.SetValue(3, mode);
			MultiplayerOptions.OptionType.RespawnPeriodTeam2.SetValue(3, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1.SetValue(0, mode);
			MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2.SetValue(0, mode);
			MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.SetValue(1, mode);
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x0007FFBC File Offset: 0x0007E1BC
		public static void InitializeFromConfigFile(string fileName)
		{
			if (!fileName.IsStringNoneOrEmpty())
			{
				string[] array = File.ReadAllLines(BasePath.Name + "Modules/Native/" + fileName);
				for (int i = 0; i < array.Length; i++)
				{
					GameNetwork.HandleConsoleCommand(array[i]);
				}
			}
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x00080000 File Offset: 0x0007E200
		public List<string> GetMultiplayerOptionsList(MultiplayerOptions.OptionType optionType)
		{
			List<string> result = new List<string>();
			switch (optionType)
			{
				case MultiplayerOptions.OptionType.GameType:
					result = (from q in Module.CurrentModule.GetMultiplayerGameTypes()
							  select q.GameType).ToList<string>();
					break;
				case MultiplayerOptions.OptionType.Map:
					result = MultiplayerGameTypes.GetGameTypeInfo(MultiplayerOptions.OptionType.GameType.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Scenes.ToList<string>();
					break;
				case MultiplayerOptions.OptionType.CultureTeam1:
				case MultiplayerOptions.OptionType.CultureTeam2:
					result = (from x in MBObjectManager.Instance.GetObjectTypeList<BasicCultureObject>()
							  select x.StringId).ToList<string>();
					break;
				default:
					if (optionType != MultiplayerOptions.OptionType.SpectatorCamera)
					{
						if (optionType == MultiplayerOptions.OptionType.AutoTeamBalanceThreshold)
						{
							List<string> list = new List<string>();
							for (int i = 0; i < 6; i++)
							{
								List<string> list2 = list;
								AutoTeamBalanceLimits autoTeamBalanceLimits = (AutoTeamBalanceLimits)i;
								list2.Add(autoTeamBalanceLimits.ToString());
							}
							result = list;
						}
					}
					else
					{
						result = new List<string>
					{
						SpectatorCameraTypes.LockToAnyAgent.ToString(),
						SpectatorCameraTypes.LockToAnyPlayer.ToString(),
						SpectatorCameraTypes.LockToTeamMembers.ToString(),
						SpectatorCameraTypes.LockToTeamMembersView.ToString()
					};
					}
					break;
			}
			return result;
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x00080151 File Offset: 0x0007E351
		private MultiplayerOptions.MultiplayerOptionsContainer GetContainer(MultiplayerOptions.MultiplayerOptionsAccessMode mode = MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)
		{
			switch (mode)
			{
				case MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions:
					return this._current;
				case MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions:
					return this._next;
				case MultiplayerOptions.MultiplayerOptionsAccessMode.MissionUIOptions:
					return this._ui;
				default:
					return null;
			}
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x0008017D File Offset: 0x0007E37D
		public void InitializeAllOptionsFromCurrent()
		{
			this._current.CopyAllValuesTo(this._ui);
			this._current.CopyAllValuesTo(this._next);
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x000801A1 File Offset: 0x0007E3A1
		public void InitializeAllOptionsFromNext()
		{
			this._next.CopyAllValuesTo(this._ui);
			this._next.CopyAllValuesTo(this._current);
			this.UpdateMbMultiplayerData();
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x000801CC File Offset: 0x0007E3CC
		public void InitializeOptionsFromUi()
		{
			this._ui.CopyAllValuesTo(this._next);
			if (Mission.Current == null)
			{
				this._ui.CopyAllValuesTo(this._current);
			}
			else
			{
				this._ui.CopyImmediateEffectValuesTo(this._current);
			}
			this.UpdateMbMultiplayerData();
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x0008021C File Offset: 0x0007E41C
		private void UpdateMbMultiplayerData()
		{
			MultiplayerOptions.MultiplayerOptionsContainer container = this.GetContainer(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			container.GetOptionFromOptionType(MultiplayerOptions.OptionType.ServerName).GetValue(out MBMultiplayerData.ServerName);
			container.GetOptionFromOptionType(MultiplayerOptions.OptionType.GameType).GetValue(out MBMultiplayerData.GameType);
			container.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map).GetValue(out MBMultiplayerData.Map);
			container.GetOptionFromOptionType(MultiplayerOptions.OptionType.MaxNumberOfPlayers).GetValue(out MBMultiplayerData.PlayerCountLimit);
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x00080278 File Offset: 0x0007E478
		public List<string> GetMapList()
		{
			MultiplayerGameTypeInfo gameTypeInfo = MultiplayerGameTypes.GetGameTypeInfo(MultiplayerOptions.OptionType.GameType.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
			List<string> list = new List<string>();
			list.Clear();
			if (gameTypeInfo.Scenes.Count > 0)
			{
				list.Add(gameTypeInfo.Scenes[0]);
				MultiplayerOptions.OptionType.Map.SetValue(list[0], MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			}
			return list;
		}

		// Token: 0x0600234A RID: 9034 RVA: 0x000802D0 File Offset: 0x0007E4D0
		public string GetValueTextForOptionWithMultipleSelection(MultiplayerOptions.OptionType optionType)
		{
			MultiplayerOptionsProperty optionProperty = optionType.GetOptionProperty();
			MultiplayerOptions.OptionValueType optionValueType = optionProperty.OptionValueType;
			if (optionValueType == MultiplayerOptions.OptionValueType.Enum)
			{
				return Enum.ToObject(optionProperty.EnumType, optionType.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).ToString();
			}
			if (optionValueType != MultiplayerOptions.OptionValueType.String)
			{
				return null;
			}
			return optionType.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
		}

		// Token: 0x0600234B RID: 9035 RVA: 0x00080318 File Offset: 0x0007E518
		public void SetValueForOptionWithMultipleSelectionFromText(MultiplayerOptions.OptionType optionType, string value)
		{
			MultiplayerOptionsProperty optionProperty = optionType.GetOptionProperty();
			MultiplayerOptions.OptionValueType optionValueType = optionProperty.OptionValueType;
			if (optionValueType != MultiplayerOptions.OptionValueType.Enum)
			{
				if (optionValueType == MultiplayerOptions.OptionValueType.String)
				{
					optionType.SetValue(value, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
				}
			}
			else
			{
				optionType.SetValue((int)Enum.Parse(optionProperty.EnumType, value), MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			}
			if (optionType == MultiplayerOptions.OptionType.GameType)
			{
				this.OnGameTypeChanged(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
			}
		}

		// Token: 0x04000CCF RID: 3279
		private const int PlayerCountLimitMin = 1;

		// Token: 0x04000CD0 RID: 3280
		private const int PlayerCountLimitMax = 120;

		// Token: 0x04000CD1 RID: 3281
		private const int PlayerCountLimitForMatchStartMin = 0;

		// Token: 0x04000CD2 RID: 3282
		private const int PlayerCountLimitForMatchStartMax = 20;

		// Token: 0x04000CD3 RID: 3283
		private const int MapTimeLimitMin = 1;

		// Token: 0x04000CD4 RID: 3284
		private const int MapTimeLimitMax = 60;

		// Token: 0x04000CD5 RID: 3285
		private const int RoundLimitMin = 1;

		// Token: 0x04000CD6 RID: 3286
		private const int RoundLimitMax = 20;

		// Token: 0x04000CD7 RID: 3287
		private const int RoundTimeLimitMin = 120;

		// Token: 0x04000CD8 RID: 3288
		private const int RoundTimeLimitMax = 960;

		// Token: 0x04000CD9 RID: 3289
		private const int RoundPreparationTimeLimitMin = 2;

		// Token: 0x04000CDA RID: 3290
		private const int RoundPreparationTimeLimitMax = 60;

		// Token: 0x04000CDB RID: 3291
		private const int RespawnPeriodMin = 1;

		// Token: 0x04000CDC RID: 3292
		private const int RespawnPeriodMax = 60;

		// Token: 0x04000CDD RID: 3293
		private const int GoldGainChangePercentageMin = -100;

		// Token: 0x04000CDE RID: 3294
		private const int GoldGainChangePercentageMax = 100;

		// Token: 0x04000CDF RID: 3295
		private const int PollAcceptThresholdMin = 0;

		// Token: 0x04000CE0 RID: 3296
		private const int PollAcceptThresholdMax = 10;

		// Token: 0x04000CE1 RID: 3297
		private const int BotsPerTeamLimitMin = 0;

		// Token: 0x04000CE2 RID: 3298
		private const int BotsPerTeamLimitMax = 100;

		// Token: 0x04000CE3 RID: 3299
		private const int BotsPerFormationLimitMin = 0;

		// Token: 0x04000CE4 RID: 3300
		private const int BotsPerFormationLimitMax = 100;

		// Token: 0x04000CE5 RID: 3301
		private const int FriendlyFireDamagePercentMin = 0;

		// Token: 0x04000CE6 RID: 3302
		private const int FriendlyFireDamagePercentMax = 100;

		// Token: 0x04000CE7 RID: 3303
		private static MultiplayerOptions _instance;

		// Token: 0x04000CE8 RID: 3304
		private readonly MultiplayerOptions.MultiplayerOptionsContainer _current;

		// Token: 0x04000CE9 RID: 3305
		private readonly MultiplayerOptions.MultiplayerOptionsContainer _next;

		// Token: 0x04000CEA RID: 3306
		private readonly MultiplayerOptions.MultiplayerOptionsContainer _ui;

		// Token: 0x02000590 RID: 1424
		public enum MultiplayerOptionsAccessMode
		{
			// Token: 0x04001BA1 RID: 7073
			CurrentMapOptions,
			// Token: 0x04001BA2 RID: 7074
			NextMapOptions,
			// Token: 0x04001BA3 RID: 7075
			MissionUIOptions
		}

		// Token: 0x02000591 RID: 1425
		public enum OptionValueType
		{
			// Token: 0x04001BA5 RID: 7077
			Bool,
			// Token: 0x04001BA6 RID: 7078
			Integer,
			// Token: 0x04001BA7 RID: 7079
			Enum,
			// Token: 0x04001BA8 RID: 7080
			String
		}

		// Token: 0x02000592 RID: 1426
		public enum OptionType
		{
			// Token: 0x04001BAA RID: 7082
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.AtMapLoad, "Changes the name of the server in the server list", 0, 0, null, false, null)]
			ServerName,
			// Token: 0x04001BAB RID: 7083
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.AtMapLoad, "Welcome messages which is shown to all players when they enter the server.", 0, 0, null, false, null)]
			WelcomeMessage,
			// Token: 0x04001BAC RID: 7084
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.Never, "Sets a password that clients have to enter before connecting to the server.", 0, 0, null, false, null)]
			GamePassword,
			// Token: 0x04001BAD RID: 7085
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.Never, "Sets a password that allows players access to admin tools during the game.", 0, 0, null, false, null)]
			AdminPassword,
			// Token: 0x04001BAE RID: 7086
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Allow players to start polls to kick other players.", 0, 0, null, false, null)]
			AllowPollsToKickPlayers,
			// Token: 0x04001BAF RID: 7087
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Allow players to start polls to ban other players.", 0, 0, null, false, null)]
			AllowPollsToBanPlayers,
			// Token: 0x04001BB0 RID: 7088
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Allow players to start polls to change the current map.", 0, 0, null, false, null)]
			AllowPollsToChangeMaps,
			// Token: 0x04001BB1 RID: 7089
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Allow players to use their custom banner.", 0, 0, null, false, null)]
			AllowIndividualBanners,
			// Token: 0x04001BB2 RID: 7090
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Use animation progress dependent blocking.", 0, 0, null, false, null)]
			UseRealisticBlocking,
			// Token: 0x04001BB3 RID: 7091
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.AtMapLoad, "Changes the game type.", 0, 0, null, true, null)]
			GameType,
			// Token: 0x04001BB4 RID: 7092
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.AtMapLoad, "Maximum player imbalance between team 1 and team 2.", 0, 0, null, true, null)]
			Map,
			// Token: 0x04001BB5 RID: 7093
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Sets culture for team 1", 0, 0, null, true, null)]
			CultureTeam1,
			// Token: 0x04001BB6 RID: 7094
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.String, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Sets culture for team 2", 0, 0, null, true, null)]
			CultureTeam2,
			// Token: 0x04001BB7 RID: 7095
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Set the maximum amount of player allowed on the server.", 1, 120, null, false, null)]
			MaxNumberOfPlayers,
			// Token: 0x04001BB8 RID: 7096
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Set the amount of players that are needed to start the first round. If not met, players will just wait.", 0, 20, null, false, null)]
			MinNumberOfPlayersForMatchStart,
			// Token: 0x04001BB9 RID: 7097
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Amount of bots on team 1", 0, 100, null, false, null)]
			NumberOfBotsTeam1,
			// Token: 0x04001BBA RID: 7098
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Amount of bots on team 2", 0, 100, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			NumberOfBotsTeam2,
			// Token: 0x04001BBB RID: 7099
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Amount of bots per formation", 0, 100, new string[]
			{
				"Captain"
			}, false, null)]
			NumberOfBotsPerFormation,
			// Token: 0x04001BBC RID: 7100
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "A percentage of how much melee damage inflicted upon a friend is dealt back to the inflictor.", 0, 100, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			FriendlyFireDamageMeleeSelfPercent,
			// Token: 0x04001BBD RID: 7101
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "A percentage of how much melee damage inflicted upon a friend is actually dealt.", 0, 100, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			FriendlyFireDamageMeleeFriendPercent,
			// Token: 0x04001BBE RID: 7102
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "A percentage of how much ranged damage inflicted upon a friend is dealt back to the inflictor.", 0, 100, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			FriendlyFireDamageRangedSelfPercent,
			// Token: 0x04001BBF RID: 7103
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "A percentage of how much ranged damage inflicted upon a friend is actually dealt.", 0, 100, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			FriendlyFireDamageRangedFriendPercent,
			// Token: 0x04001BC0 RID: 7104
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Enum, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Who can spectators look at, and how.", 0, 7, null, true, typeof(SpectatorCameraTypes))]
			SpectatorCamera,
			// Token: 0x04001BC1 RID: 7105
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.AtMapLoad, "Maximum duration for the warmup. In minutes.", 1, 60, null, false, null)]
			WarmupTimeLimit,
			// Token: 0x04001BC2 RID: 7106
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Maximum duration for the map. In minutes.", 1, 60, null, false, null)]
			MapTimeLimit,
			// Token: 0x04001BC3 RID: 7107
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Maximum duration for each round. In seconds.", 120, 960, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege"
			}, false, null)]
			RoundTimeLimit,
			// Token: 0x04001BC4 RID: 7108
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Time available to select class/equipment. In seconds.", 2, 60, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege"
			}, false, null)]
			RoundPreparationTimeLimit,
			// Token: 0x04001BC5 RID: 7109
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Maximum amount of rounds before the game ends.", 1, 20, new string[]
			{
				"Battle",
				"NewBattle",
				"ClassicBattle",
				"Captain",
				"Skirmish",
				"Siege"
			}, false, null)]
			RoundTotal,
			// Token: 0x04001BC6 RID: 7110
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Wait time after death, before respawning again. In seconds.", 1, 60, new string[]
			{
				"Siege"
			}, false, null)]
			RespawnPeriodTeam1,
			// Token: 0x04001BC7 RID: 7111
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Wait time after death, before respawning again. In seconds.", 1, 60, new string[]
			{
				"Siege"
			}, false, null)]
			RespawnPeriodTeam2,
			// Token: 0x04001BC8 RID: 7112
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Gold gain multiplier from agent deaths.", -100, 100, new string[]
			{
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			GoldGainChangePercentageTeam1,
			// Token: 0x04001BC9 RID: 7113
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Gold gain multiplier from agent deaths.", -100, 100, new string[]
			{
				"Siege",
				"TeamDeathmatch"
			}, false, null)]
			GoldGainChangePercentageTeam2,
			// Token: 0x04001BCA RID: 7114
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Min score to win match.", 0, 120000, new string[]
			{
				"TeamDeathmatch"
			}, false, null)]
			MinScoreToWinMatch,
			// Token: 0x04001BCB RID: 7115
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Integer, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Minimum needed difference in poll results before it is accepted.", 0, 10, null, false, null)]
			PollAcceptThreshold,
			// Token: 0x04001BCC RID: 7116
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Enum, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Maximum player imbalance between team 1 and team 2.", 0, 5, null, true, typeof(AutoTeamBalanceLimits))]
			AutoTeamBalanceThreshold,
			// Token: 0x04001BCD RID: 7117
			[MultiplayerOptionsProperty(MultiplayerOptions.OptionValueType.Bool, MultiplayerOptionsProperty.ReplicationOccurrence.Immediately, "Enables anti-cheat.", 0, 0, null, false, null)]
			EnableAntiCheat,
			// Token: 0x04001BCE RID: 7118
			NumOfSlots
		}

		// Token: 0x02000593 RID: 1427
		public class MultiplayerOption
		{
			// Token: 0x0600346E RID: 13422 RVA: 0x000BD0EB File Offset: 0x000BB2EB
			public static MultiplayerOptions.MultiplayerOption CreateMultiplayerOption(MultiplayerOptions.OptionType optionType)
			{
				return new MultiplayerOptions.MultiplayerOption(optionType);
			}

			// Token: 0x0600346F RID: 13423 RVA: 0x000BD0F4 File Offset: 0x000BB2F4
			private MultiplayerOption(MultiplayerOptions.OptionType optionType)
			{
				this.OptionType = optionType;
				if (optionType.GetOptionProperty().OptionValueType == MultiplayerOptions.OptionValueType.String)
				{
					this._intValue = MultiplayerOptions.MultiplayerOption.IntegerValue.Invalid;
					this._stringValue = MultiplayerOptions.MultiplayerOption.StringValue.Create();
					return;
				}
				this._intValue = MultiplayerOptions.MultiplayerOption.IntegerValue.Create();
				this._stringValue = MultiplayerOptions.MultiplayerOption.StringValue.Invalid;
			}

			// Token: 0x06003470 RID: 13424 RVA: 0x000BD149 File Offset: 0x000BB349
			public MultiplayerOptions.MultiplayerOption UpdateValue(bool value)
			{
				this.UpdateValue(value ? 1 : 0);
				return this;
			}

			// Token: 0x06003471 RID: 13425 RVA: 0x000BD15A File Offset: 0x000BB35A
			public MultiplayerOptions.MultiplayerOption UpdateValue(int value)
			{
				this._intValue.UpdateValue(value);
				return this;
			}

			// Token: 0x06003472 RID: 13426 RVA: 0x000BD169 File Offset: 0x000BB369
			public MultiplayerOptions.MultiplayerOption UpdateValue(string value)
			{
				this._stringValue.UpdateValue(value);
				return this;
			}

			// Token: 0x06003473 RID: 13427 RVA: 0x000BD178 File Offset: 0x000BB378
			public void GetValue(out bool value)
			{
				value = (this._intValue.Value == 1);
			}

			// Token: 0x06003474 RID: 13428 RVA: 0x000BD18A File Offset: 0x000BB38A
			public void GetValue(out int value)
			{
				value = this._intValue.Value;
			}

			// Token: 0x06003475 RID: 13429 RVA: 0x000BD199 File Offset: 0x000BB399
			public void GetValue(out string value)
			{
				value = this._stringValue.Value;
			}

			// Token: 0x04001BCF RID: 7119
			public readonly MultiplayerOptions.OptionType OptionType;

			// Token: 0x04001BD0 RID: 7120
			private MultiplayerOptions.MultiplayerOption.IntegerValue _intValue;

			// Token: 0x04001BD1 RID: 7121
			private MultiplayerOptions.MultiplayerOption.StringValue _stringValue;

			// Token: 0x02000629 RID: 1577
			private struct IntegerValue
			{
				// Token: 0x17000887 RID: 2183
				// (get) Token: 0x06003630 RID: 13872 RVA: 0x000BFAA4 File Offset: 0x000BDCA4
				public static MultiplayerOptions.MultiplayerOption.IntegerValue Invalid
				{
					get
					{
						return default(MultiplayerOptions.MultiplayerOption.IntegerValue);
					}
				}

				// Token: 0x17000888 RID: 2184
				// (get) Token: 0x06003631 RID: 13873 RVA: 0x000BFABA File Offset: 0x000BDCBA
				// (set) Token: 0x06003632 RID: 13874 RVA: 0x000BFAC2 File Offset: 0x000BDCC2
				public bool IsValid { get; private set; }

				// Token: 0x17000889 RID: 2185
				// (get) Token: 0x06003633 RID: 13875 RVA: 0x000BFACB File Offset: 0x000BDCCB
				// (set) Token: 0x06003634 RID: 13876 RVA: 0x000BFAD3 File Offset: 0x000BDCD3
				public int Value { get; private set; }

				// Token: 0x06003635 RID: 13877 RVA: 0x000BFADC File Offset: 0x000BDCDC
				public static MultiplayerOptions.MultiplayerOption.IntegerValue Create()
				{
					return new MultiplayerOptions.MultiplayerOption.IntegerValue
					{
						IsValid = true
					};
				}

				// Token: 0x06003636 RID: 13878 RVA: 0x000BFAFA File Offset: 0x000BDCFA
				public void UpdateValue(int value)
				{
					this.Value = value;
				}
			}

			// Token: 0x0200062A RID: 1578
			private struct StringValue
			{
				// Token: 0x1700088A RID: 2186
				// (get) Token: 0x06003637 RID: 13879 RVA: 0x000BFB04 File Offset: 0x000BDD04
				public static MultiplayerOptions.MultiplayerOption.StringValue Invalid
				{
					get
					{
						return default(MultiplayerOptions.MultiplayerOption.StringValue);
					}
				}

				// Token: 0x1700088B RID: 2187
				// (get) Token: 0x06003638 RID: 13880 RVA: 0x000BFB1A File Offset: 0x000BDD1A
				// (set) Token: 0x06003639 RID: 13881 RVA: 0x000BFB22 File Offset: 0x000BDD22
				public bool IsValid { get; private set; }

				// Token: 0x1700088C RID: 2188
				// (get) Token: 0x0600363A RID: 13882 RVA: 0x000BFB2B File Offset: 0x000BDD2B
				// (set) Token: 0x0600363B RID: 13883 RVA: 0x000BFB33 File Offset: 0x000BDD33
				public string Value { get; private set; }

				// Token: 0x0600363C RID: 13884 RVA: 0x000BFB3C File Offset: 0x000BDD3C
				public static MultiplayerOptions.MultiplayerOption.StringValue Create()
				{
					return new MultiplayerOptions.MultiplayerOption.StringValue
					{
						IsValid = true
					};
				}

				// Token: 0x0600363D RID: 13885 RVA: 0x000BFB5A File Offset: 0x000BDD5A
				public void UpdateValue(string value)
				{
					this.Value = value;
				}
			}
		}

		// Token: 0x02000594 RID: 1428
		private class MultiplayerOptionsContainer
		{
			// Token: 0x06003476 RID: 13430 RVA: 0x000BD1A8 File Offset: 0x000BB3A8
			public MultiplayerOptionsContainer()
			{
				this._multiplayerOptions = new MultiplayerOptions.MultiplayerOption[36];
			}

			// Token: 0x06003477 RID: 13431 RVA: 0x000BD1BD File Offset: 0x000BB3BD
			public MultiplayerOptions.MultiplayerOption GetOptionFromOptionType(MultiplayerOptions.OptionType optionType)
			{
				return this._multiplayerOptions[(int)optionType];
			}

			// Token: 0x06003478 RID: 13432 RVA: 0x000BD1C7 File Offset: 0x000BB3C7
			private void CopyOptionFromOther(MultiplayerOptions.OptionType optionType, MultiplayerOptions.MultiplayerOption option)
			{
				this._multiplayerOptions[(int)optionType] = option;
			}

			// Token: 0x06003479 RID: 13433 RVA: 0x000BD1D2 File Offset: 0x000BB3D2
			public void CreateOption(MultiplayerOptions.OptionType optionType)
			{
				this._multiplayerOptions[(int)optionType] = MultiplayerOptions.MultiplayerOption.CreateMultiplayerOption(optionType);
			}

			// Token: 0x0600347A RID: 13434 RVA: 0x000BD1E2 File Offset: 0x000BB3E2
			public void UpdateOptionValue(MultiplayerOptions.OptionType optionType, int value)
			{
				this._multiplayerOptions[(int)optionType].UpdateValue(value);
			}

			// Token: 0x0600347B RID: 13435 RVA: 0x000BD1F3 File Offset: 0x000BB3F3
			public void UpdateOptionValue(MultiplayerOptions.OptionType optionType, string value)
			{
				this._multiplayerOptions[(int)optionType].UpdateValue(value);
			}

			// Token: 0x0600347C RID: 13436 RVA: 0x000BD204 File Offset: 0x000BB404
			public void UpdateOptionValue(MultiplayerOptions.OptionType optionType, bool value)
			{
				this._multiplayerOptions[(int)optionType].UpdateValue(value ? 1 : 0);
			}

			// Token: 0x0600347D RID: 13437 RVA: 0x000BD21B File Offset: 0x000BB41B
			public void CopyAllValuesTo(MultiplayerOptions.MultiplayerOptionsContainer other)
			{
				this.CopyImmediateEffectValuesTo(other);
				this.CopyNewRoundValuesTo(other);
				this.CopyNewMapValuesTo(other);
			}

			// Token: 0x0600347E RID: 13438 RVA: 0x000BD234 File Offset: 0x000BB434
			public void CopyImmediateEffectValuesTo(MultiplayerOptions.MultiplayerOptionsContainer other)
			{
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AllowPollsToKickPlayers, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AllowPollsToKickPlayers));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AllowPollsToBanPlayers, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AllowPollsToBanPlayers));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AllowPollsToChangeMaps, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AllowPollsToChangeMaps));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AllowIndividualBanners, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AllowIndividualBanners));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.UseRealisticBlocking, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.UseRealisticBlocking));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.MaxNumberOfPlayers, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.MaxNumberOfPlayers));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.WarmupTimeLimit, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.WarmupTimeLimit));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.MapTimeLimit, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.MapTimeLimit));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.RoundTotal, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.RoundTotal));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.RoundTimeLimit, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.RoundTimeLimit));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.RoundPreparationTimeLimit, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.RoundPreparationTimeLimit));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.RespawnPeriodTeam1, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.RespawnPeriodTeam1));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.RespawnPeriodTeam2, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.RespawnPeriodTeam2));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.GoldGainChangePercentageTeam1));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.GoldGainChangePercentageTeam2));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.MinScoreToWinMatch, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.MinScoreToWinMatch));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.PollAcceptThreshold, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.PollAcceptThreshold));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.SpectatorCamera, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.SpectatorCamera));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.NumberOfBotsTeam1, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.NumberOfBotsTeam1));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.NumberOfBotsTeam2, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.NumberOfBotsTeam2));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AutoTeamBalanceThreshold, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AutoTeamBalanceThreshold));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.WelcomeMessage, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.WelcomeMessage));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.GamePassword, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.GamePassword));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.AdminPassword, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.AdminPassword));
			}

			// Token: 0x0600347F RID: 13439 RVA: 0x000BD401 File Offset: 0x000BB601
			public void CopyNewRoundValuesTo(MultiplayerOptions.MultiplayerOptionsContainer other)
			{
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.NumberOfBotsPerFormation, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.NumberOfBotsPerFormation));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.CultureTeam1, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.CultureTeam2, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2));
			}

			// Token: 0x06003480 RID: 13440 RVA: 0x000BD433 File Offset: 0x000BB633
			public void CopyNewMapValuesTo(MultiplayerOptions.MultiplayerOptionsContainer other)
			{
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.Map, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map));
				other.CopyOptionFromOther(MultiplayerOptions.OptionType.GameType, this.GetOptionFromOptionType(MultiplayerOptions.OptionType.GameType));
			}

			// Token: 0x04001BD2 RID: 7122
			private readonly MultiplayerOptions.MultiplayerOption[] _multiplayerOptions;
		}
	}
}
*/