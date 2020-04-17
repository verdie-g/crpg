using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod
{

	// Token: 0x02000003 RID: 3
	internal class CombatTestMissionController : MissionView
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020C4 File Offset: 0x000002C4
		public override void OnAfterMissionCreated()
		{
			base.OnAfterMissionCreated();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020CE File Offset: 0x000002CE
		private static void DisplayMessage(string msg)
		{
			InformationManager.DisplayMessage(new InformationMessage(new TextObject(msg, null).ToString()));
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000020E8 File Offset: 0x000002E8
		public override void OnMissionTick(float dt)
		{
			base.OnMissionTick(dt);
			this.RefreshGUI(dt);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020FC File Offset: 0x000002FC
		private void RefreshGUI(float dt)
		{
			Imgui.BeginMainThreadScope();
			Imgui.Begin("Combat Dev Feedback Reload");
			bool flag = Imgui.Button("Reload Managed Core Params");
			if (flag)
			{
				ManagedParameters.Instance.Initialize(ModuleInfo.GetXmlPath("CombatDevTest", "managed_core_parameters"));
				CombatTestMissionController.DisplayMessage("Reloaded managed Core Params");
			}
			this.DrawReloadXMLs();
			Imgui.End();
			Imgui.Begin("Combat Dev Feedback Cheats");
			bool flag2 = Imgui.Button(" Quit");
			if (flag2)
			{
				GameStateManager gameStateManager = Game.Current.GameStateManager;
				bool flag3 = !(gameStateManager.ActiveState is LobbyState);
				if (flag3)
				{
					bool flag4 = gameStateManager.ActiveState is MissionState;
					if (flag4)
					{
						Imgui.End();
						Imgui.EndMainThreadScope();
						NetworkMain.GameClient.Logout();
						return;
					}
					gameStateManager.PopState(0);
				}
			}
			this.DrawDevCheats();
			Imgui.End();
			Imgui.EndMainThreadScope();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000021E4 File Offset: 0x000003E4
		private void DrawDevCheats()
		{
			this.wasEnableAIChanges = this.enableAIChanges;
			Imgui.Checkbox("Player Invulnerable", ref this.playerInvulnerable);
			Agent player = Mission.Current.MainAgent;
			Imgui.Checkbox(string.Format("Everyone Invulnerable?: {0}", this._allInvulnerable), ref this._allInvulnerable);
			Imgui.Checkbox(string.Format("Everyone Passive?: {0}", this.everyonePassive), ref this.everyonePassive);
			Imgui.Checkbox("Enable AI Changes", ref this.enableAIChanges);
			foreach (Agent agent in Mission.Current.AllAgents)
			{
				bool flag = agent == null;
				if (!flag)
				{
					if (agent != null)
					{
						agent.SetInvulnerable(this._allInvulnerable);
					}
					AgentAIStateFlagComponent component = (agent != null) ? agent.GetComponent<AgentAIStateFlagComponent>() : null;
					bool flag2 = component != null;
					if (flag2)
					{
						component.IsPaused = this.everyonePassive;
					}
					bool flag3 = agent == player;
					if (flag3)
					{
					}
				}
			}
			bool flag4 = this.enableAIChanges;
			if (flag4)
			{
				bool flag5 = this.wasEnableAIChanges;
				if (flag5)
				{
					this.SliderUpdate();
					this.AskForApply();
				}
				else
				{
					this.BackupStats();
				}
			}
			else
			{
				bool flag6 = this.wasEnableAIChanges;
				if (flag6)
				{
					this.ResetStats();
				}
			}
			bool flag7 = Imgui.Button(" Gib Player 100 Money");
			if (flag7)
			{
				MissionMultiplayerGameModeBase _gameModeServer = Mission.Current.GetMissionBehaviour<MissionMultiplayerGameModeBase>();
				_gameModeServer.ChangeCurrentGoldForPeer(GameNetwork.MyPeer.GetComponent<MissionPeer>(), _gameModeServer.GetCurrentGoldForPeer(GameNetwork.MyPeer.GetComponent<MissionPeer>()) + 100);
			}
			if (player != null)
			{
				player.SetInvulnerable(this.playerInvulnerable);
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000023A0 File Offset: 0x000005A0
		private void ResetStats()
		{
			Agent player = Mission.Current.MainAgent;
			foreach (Agent agent in Mission.Current.AllAgents)
			{
				bool flag = agent == null || agent == player;
				if (!flag)
				{
					AgentAIStateFlagComponent component = (agent != null) ? agent.GetComponent<AgentAIStateFlagComponent>() : null;
					bool flag2 = component != null;
					if (flag2)
					{
						component.IsPaused = this.everyonePassive;
					}
					PropertyInfo property = agent.GetType().GetProperty("AgentDrivenProperties", BindingFlags.Instance | BindingFlags.NonPublic);
					AgentDrivenProperties agentDrivenProperties = (AgentDrivenProperties)((property != null) ? property.GetValue(agent) : null);
					foreach (DrivenProperty drivenProperty in (DrivenProperty[])Enum.GetValues(typeof(DrivenProperty)))
					{
						bool flag3 = drivenProperty >= DrivenProperty.AIHoldingReadyMaxDuration || drivenProperty <= DrivenProperty.None;
						if (!flag3)
						{
							float val = this.StatsBackup.GetStat(drivenProperty);
							if (agentDrivenProperties != null)
							{
								agentDrivenProperties.SetStat(drivenProperty, val);
							}
						}
					}
					agent.UpdateAgentProperties();
				}
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000024D4 File Offset: 0x000006D4
		private void BackupStats()
		{
			Agent player = Mission.Current.MainAgent;
			foreach (Agent agent in Mission.Current.AllAgents)
			{
				bool flag = agent == null || agent == player;
				if (!flag)
				{
					AgentAIStateFlagComponent component = (agent != null) ? agent.GetComponent<AgentAIStateFlagComponent>() : null;
					bool flag2 = component != null;
					if (flag2)
					{
						component.IsPaused = this.everyonePassive;
					}
					PropertyInfo property = agent.GetType().GetProperty("AgentDrivenProperties", BindingFlags.Instance | BindingFlags.NonPublic);
					AgentDrivenProperties agentDrivenProperties = (AgentDrivenProperties)((property != null) ? property.GetValue(agent) : null);
					bool flag3 = agentDrivenProperties == null;
					if (!flag3)
					{
						foreach (DrivenProperty drivenProperty in (DrivenProperty[])Enum.GetValues(typeof(DrivenProperty)))
						{
							bool flag4 = drivenProperty >= DrivenProperty.AIHoldingReadyMaxDuration || drivenProperty <= DrivenProperty.None;
							if (!flag4)
							{
								float val = agentDrivenProperties.GetStat(drivenProperty);
								this.StatsBackup.SetStat(drivenProperty, val);
								this.StatsToSet.SetStat(drivenProperty, val);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002618 File Offset: 0x00000818
		private void AskForApply()
		{
			Agent player = Mission.Current.MainAgent;
			bool flag = Imgui.Button("UPDATE AI");
			if (flag)
			{
				foreach (Agent agent in Mission.Current.AllAgents)
				{
					bool flag2 = agent == null || agent == player;
					if (!flag2)
					{
						AgentAIStateFlagComponent component = (agent != null) ? agent.GetComponent<AgentAIStateFlagComponent>() : null;
						bool flag3 = component != null;
						if (flag3)
						{
							component.IsPaused = this.everyonePassive;
						}
						PropertyInfo property = agent.GetType().GetProperty("AgentDrivenProperties", BindingFlags.Instance | BindingFlags.NonPublic);
						AgentDrivenProperties agentDrivenProperties = (AgentDrivenProperties)((property != null) ? property.GetValue(agent) : null);
						foreach (DrivenProperty drivenProperty in (DrivenProperty[])Enum.GetValues(typeof(DrivenProperty)))
						{
							bool flag4 = drivenProperty < DrivenProperty.AIHoldingReadyMaxDuration && drivenProperty > DrivenProperty.None;
							if (flag4)
							{
								float val = this.StatsToSet.GetStat(drivenProperty);
								if (agentDrivenProperties != null)
								{
									agentDrivenProperties.SetStat(drivenProperty, val);
								}
							}
							agent.UpdateAgentProperties();
						}
					}
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002760 File Offset: 0x00000960
		private void SliderUpdate()
		{
			foreach (DrivenProperty drivenProperty in (DrivenProperty[])Enum.GetValues(typeof(DrivenProperty)))
			{
				bool flag = drivenProperty < DrivenProperty.AIHoldingReadyMaxDuration && drivenProperty > DrivenProperty.None;
				if (flag)
				{
					float val = this.StatsToSet.GetStat(drivenProperty);
					Imgui.SliderFloat(Enum.GetName(typeof(DrivenProperty), drivenProperty), ref val, -10f, 10f);
					this.StatsToSet.SetStat(drivenProperty, val);
				}
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000027F0 File Offset: 0x000009F0
		private void DrawReloadXMLs()
		{
			foreach (string xml in new string[]
			{
				"BasicCultures",
				"MPCharacters",
				"MPClassDivisions",
				"Monsters",
				"SkeletonScales",
				"ItemModifiers",
				"ItemModifierGroups",
				"CraftingPieces",
				"CraftingTemplates",
				"Items"
			})
			{
				this.drawReloadXML(xml);
				Imgui.Separator();
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x0000287C File Offset: 0x00000A7C
		private void drawReloadXML(string xmlFile)
		{
			bool flag = Imgui.Button("Reload " + xmlFile);
			if (flag)
			{
				MBObjectManager.Instance.LoadXML(xmlFile, null, false);
			}
		}

		// Token: 0x04000001 RID: 1
		private static List<string> consoleCommands;

		// Token: 0x04000002 RID: 2
		private bool _allInvulnerable = false;

		// Token: 0x04000003 RID: 3
		private bool enableAIChanges = false;

		// Token: 0x04000004 RID: 4
		private bool everyonePassive = false;

		// Token: 0x04000005 RID: 5
		private bool playerInvulnerable = false;

		// Token: 0x04000006 RID: 6
		private AgentDrivenProperties StatsBackup = new AgentDrivenProperties();

		// Token: 0x04000007 RID: 7
		private AgentDrivenProperties StatsToSet = new AgentDrivenProperties();

		// Token: 0x04000008 RID: 8
		private bool wasEnableAIChanges = false;
	}
}
