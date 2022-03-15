using SandBox;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities;
using TOW_Core.Battle.CrosshairMissionBehavior;
using TOW_Core.Battle.Crosshairs;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORAgentStatCalculateModel : SandboxAgentStatCalculateModel
    {
        private float vampireDaySpeedModificator = 1.1f;
        private float vampireNightSpeedModificator = 1.2f;
        private CustomCrosshairMissionBehavior _crosshairBehavior;


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

		//The purpose of this is to prevent an invalid type cast from IAgentOriginBase to PartybBase for SummonedAgentOrigin troops.
        public override void InitializeMissionEquipment(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return;
            else base.InitializeMissionEquipment(agent);
		}

        public override int GetEffectiveSkill(BasicCharacterObject agentCharacter, IAgentOriginBase agentOrigin, Formation agentFormation, SkillObject skill)
        {
            if (agentOrigin is SummonedAgentOrigin) return agentCharacter.GetSkillValue(skill);
            else return base.GetEffectiveSkill(agentCharacter, agentOrigin, agentFormation, skill);
        }

        public override string GetMissionDebugInfoForAgent(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return "Impossible to debug summoned units. Base implementation has invalid IAgentOriginBase to PartyBase type cast.";
            else return base.GetMissionDebugInfoForAgent(agent);
        }

        public override float GetEffectiveMaxHealth(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return agent.BaseHealthLimit;
            else return base.GetEffectiveMaxHealth(agent);
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

        public override float GetMaxCameraZoom(Agent agent)
        {
            if (_crosshairBehavior == null)
            {
                _crosshairBehavior = Mission.Current.GetMissionBehavior<CustomCrosshairMissionBehavior>();
            }

            if (_crosshairBehavior != null && _crosshairBehavior.CurrentCrosshair is SniperScope && _crosshairBehavior.CurrentCrosshair.IsVisible)
            {
                return 3;
            }
            return base.GetMaxCameraZoom(agent);
        }
    }
}
