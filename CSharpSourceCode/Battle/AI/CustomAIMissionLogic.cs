using System.Linq;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.AI
{
    public class CustomAiMissionLogic : MissionLogic
    {
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (agent != Agent.Main && agent.IsAbilityUser())
            {
                var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
                foreach (var item in toRemove)
                    agent.RemoveComponent(item);
                TOWCommon.Say("Added Wizard component");
                agent.AddComponent(new WizardAiComponent(agent));
            }
        }
    }
}

