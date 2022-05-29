using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class KnockDownScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents)
        {
            foreach(Agent agent in triggeredAgents)
            {
                if (agent != null && (agent.State == TaleWorlds.Core.AgentState.Active || agent.State == TaleWorlds.Core.AgentState.Routed))
                {
                    agent.FallDown();
                }
            }
        }
    }
}
