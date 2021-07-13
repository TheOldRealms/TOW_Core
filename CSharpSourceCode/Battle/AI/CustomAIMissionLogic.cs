using System.Linq;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.AI.Components;
using TOW_Core.Battle.Extensions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.AI
{
    public class CustomAIMissionLogic : MissionLogic
    {
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            if (agent.IsAIControlled && agent.IsAbilityUser())
            {
                var toRemove = agent.Components.OfType<HumanAIComponent>().ToList();
                foreach (var item in toRemove)
                    agent.RemoveComponent(item);
                if (toRemove.Count > 0)
                {
                    agent.AddComponent(new WizardAIComponent(agent));
                }
            }
        }
    }
}