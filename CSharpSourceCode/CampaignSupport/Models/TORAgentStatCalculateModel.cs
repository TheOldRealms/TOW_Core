using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORAgentStatCalculateModel : SandboxAgentStatCalculateModel
    {
        private float vampireDaySpeedModificator = 1.1f;
        private float vampireNightSpeedModificator = 1.2f;

        public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            base.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
            UpdateAgentDrivenProperties(agent, agentDrivenProperties);
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            UpdateAgentDrivenProperties(agent, agentDrivenProperties);
        }

        private void UpdateAgentDrivenProperties(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {            
            if (agent.IsHuman)
            {
                var character = agent.Character as CharacterObject;
                if (character != null)
                {
                    if (character.IsHero)
                    {
                        if (character.HeroObject.IsVampire())
                        {
                            float modificator = vampireDaySpeedModificator; 
                            if (Campaign.Current != null && Campaign.Current.IsNight)
                            {
                                modificator = vampireNightSpeedModificator;
                            }
                            agentDrivenProperties.TopSpeedReachDuration *= modificator;
                            agentDrivenProperties.MaxSpeedMultiplier *= modificator;
                            agentDrivenProperties.CombatMaxSpeedMultiplier *= modificator;
                        }
                    }
                }
            }
        }
    }
}
