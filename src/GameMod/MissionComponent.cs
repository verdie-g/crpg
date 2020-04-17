using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlatformService;
using TaleWorlds.PlatformService.Steam;
using TaleWorlds.PlayerServices;
using TaleWorlds.Diamond.AccessProvider.Steam;
using Steamworks;

namespace Crpg.GameMod
{
	public class MissionComponent : MissionLogic
	{

		//OnAgentCreated/AgentBuild/OnAgentDeleted

		/*public override void OnAfterMissionCreated()
		{
			base.OnAfterMissionCreated();
			InformationManager.DisplayMessage(new InformationMessage("OnAfterMissionCreated"));
		}*/
		public override void OnAgentCreated(Agent agent)
		{
			base.OnAgentCreated(agent);
			InformationManager.DisplayMessage(new InformationMessage("OnAgentCreated"));
			bool isFemale = agent.IsFemale;
			InformationManager.DisplayMessage(new InformationMessage("OnAgentCreated" + isFemale));
			/*CampaignAgentComponent agentComponent = new CampaignAgentComponent(agent);
			agent.AddComponent(agentComponent);
			if (agent.Character != null)
			{
				CharacterObject characterObject = (CharacterObject)agent.Character;
				if (characterObject.HeroObject != null && characterObject.HeroObject.IsPlayerCompanion)
				{
					agent.AgentRole = new TextObject("{=kPTp6TPT}({AGENT_ROLE})", null);
					agent.AgentRole.SetTextVariable("AGENT_ROLE", GameTexts.FindText("str_companion", null));
				}
			}*/
		}
		/*public override void OnAgentDeleted(Agent agent)
		{
			base.OnAgentDeleted(agent);
			InformationManager.DisplayMessage(new InformationMessage("OnAgentDeleted"));
		}*/
		public override void OnAgentBuild(Agent agent, Banner banner)
		{
			base.OnAgentBuild(agent, banner);
			InformationManager.DisplayMessage(new InformationMessage("OnAgentBuild"));
		}
		public override void OnMissionTick(float dt)
		{
			base.OnMissionTick(dt);
			//InformationManager.DisplayMessage(new InformationMessage("OnMissionTick"));

			/*if (Campaign.Current != null)
			{
				CampaignEventDispatcher.Instance.MissionTick(dt);
			}
			if (this._soundEvent != null && !this._soundEvent.IsPlaying())
			{
				this.RemovePreviousAgentsSoundEvent();
				this._soundEvent.Stop();
				this._soundEvent = null;
			}
			if (base.Mission.Mode == MissionMode.Conversation || base.Mission.Mode == MissionMode.Barter)
			{
				this.HandleAnimations();
			}*/
		}



		// Token: 0x060002C3 RID: 707 RVA: 0x00017032 File Offset: 0x00015232
		public override void OnCreated()
		{
			InformationManager.DisplayMessage(new InformationMessage("OnCreated"));
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0001704C File Offset: 0x0001524C
		public override void OnBehaviourInitialize()
		{
			base.OnBehaviourInitialize();
			InformationManager.DisplayMessage(new InformationMessage("OnBehaviourInitialize"));
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00017064 File Offset: 0x00015264
		public override void AfterStart()
		{
			base.AfterStart();
		}
		// Token: 0x060002C8 RID: 712 RVA: 0x0001715F File Offset: 0x0001535F
		protected override void OnEndMission()
		{
	
		}


	}
}
