using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.ObjectDataExtensions.CustomAgentComponents;
using TaleWorlds.CampaignSystem;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Abilities;
using TOW_Core.Battle.AI.Components;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomMissionLogic
{
    class AttributeSystemMissionLogic : MissionLogic
    {
        public AttributeSystemMissionLogic()
        {
        }

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (agent.IsUndead())
            {
                agent.AddComponent(new UndeadMoraleAgentComponent(agent));
            }
            if (agent.IsAbilityUser())
            {
                agent.AddComponent(new AbilityComponent(agent));
                if (agent.IsAIControlled)
                {
                    agent.AddComponent(new WizardAIComponent(agent));
                }
            }

        }
    }
}
