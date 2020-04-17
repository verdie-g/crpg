using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod
{
    public class MissionComponent : MissionLogic
    {
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            InformationManager.DisplayMessage(new InformationMessage("OnAgentCreated"));
            /*
            CampaignAgentComponent agentComponent = new CampaignAgentComponent(agent);
            agent.AddComponent(agentComponent);
            if (agent.Character != null)
            {
                CharacterObject characterObject = (CharacterObject)agent.Character;
                if (characterObject.HeroObject != null && characterObject.HeroObject.IsPlayerCompanion)
                {
                    agent.AgentRole = new TextObject("{=kPTp6TPT}({AGENT_ROLE})", null);
                    agent.AgentRole.SetTextVariable("AGENT_ROLE", GameTexts.FindText("str_companion", null));
                }
            }
            */
        }

        public override void OnAgentDeleted(Agent agent)
        {
            base.OnAgentDeleted(agent);
            InformationManager.DisplayMessage(new InformationMessage("OnAgentDeleted"));
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            InformationManager.DisplayMessage(new InformationMessage("OnAgentBuild"));
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            /*
            InformationManager.DisplayMessage(new InformationMessage("OnMissionTick"));
            if (Campaign.Current != null)
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
            }
            */
        }

        public override void OnCreated()
        {
        }

        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
        }

        public override void AfterStart()
        {
            base.AfterStart();
        }

        protected override void OnEndMission()
        {
        }
    }
}
