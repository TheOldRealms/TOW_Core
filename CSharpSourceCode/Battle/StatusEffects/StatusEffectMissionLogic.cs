using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectMissionLogic : MissionLogic
    {
        public List<Agent> activeAgents;
        private bool init;

      
        public override void OnAgentCreated(Agent agent)
        {
            if (activeAgents == null&&!init)
            {
                activeAgents = new List<Agent>();
                init = true;
            }
            
            if (agent.IsHuman)
            {
                StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
                agent.AddComponent(effectComponent);
                activeAgents.Add(agent);
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            
            if (!init)
                return;
            
            foreach(var agent in activeAgents.ToList())
            {
                if (agent.GetComponent<StatusEffectComponent>() != null)
                {
                    if (agent.IsActive() && agent.Health > 1f)
                    {
                        var comp = agent.GetComponent<StatusEffectComponent>();
                        comp.OnTick(dt);
                    }

                    if (agent.GetComponent<StatusEffectComponent>().IsDisabled())
                    {
                        activeAgents.Remove(agent);
                    }
                }
            }
        }

        public void RemoveAgent(Agent agent, Blow test)
        {
            if (activeAgents.Contains(agent))
            {
                agent.GetComponent<StatusEffectComponent>().RenderDisabled(true);
                //agent.Die(test);
            }
            
            
        }
    }
}