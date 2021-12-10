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
        private Dictionary<int, Agent> _activeAgents;
        private bool init;

      
        public override void OnAgentCreated(Agent agent)
        {
            if (_activeAgents == null&&!init)
            {
                _activeAgents = new Dictionary<int, Agent>(); 
                init = true;
            }
            
            if (agent.IsHuman)
            {
                StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
                agent.AddComponent(effectComponent);
                _activeAgents?.Add(agent.Index, agent);
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            
            if (!init)
                return;

            foreach (var agent in _activeAgents.ToDictionary(x =>x.Key, x=> x.Value))
            {
                if (agent.Value.GetComponent<StatusEffectComponent>() != null)
                {
                    if (agent.Value.GetComponent<StatusEffectComponent>().IsDisabled())
                    {
                        _activeAgents.Remove(agent.Key);
                        continue;
                    }
                    if (agent.Value.IsActive() && agent.Value.Health > 1f)
                    {
                        var comp = agent.Value.GetComponent<StatusEffectComponent>();
                        comp.OnTick(dt);
                    }

                    
                }
            }
        }

        public void RemoveAgent(Agent agent)
        {
            if (_activeAgents.Values.Contains(agent))
            {
                agent.GetComponent<StatusEffectComponent>().RenderDisabled(true);
            }
            
        }
    }
}