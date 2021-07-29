using System;
using System.Collections.Generic;
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
        private Dictionary<string, StatusEffect> _presentEffects = new Dictionary<string, StatusEffect>();

        public EventHandler<OnTickArgs> NotifyStatusEffectTickObservers;
        
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
            agent.AddComponent(effectComponent);
            if(!_presentEffects.ContainsKey("crumble") && agent.IsUndead())
            {
                _presentEffects.Add("crumble", StatusEffectManager.GetStatusEffect("crumble"));
            }
        }
        
        public override void OnAfterMissionCreated()
        {
            base.OnAfterMissionCreated();
            var attacker = Mission.Current.Teams.GetAlliesOf(Mission.Current.Teams.Attacker, true);
            var defender = Mission.Current.Teams.GetAlliesOf(Mission.Current.Teams.Defender, true);
            var agents = Mission.Current.Agents;
            //assign Team allies to Parties

            foreach (var team in attacker)
            {
                foreach (Agent agent in team.TeamAgents)
                {
                    if(agent.Character.IsPlayerCharacter||agent.Character.IsHero)
                        TOWCommon.Say(agent.Name);
                    
                    if (agent.IsHero|| agent.IsMainAgent)
                    {
                        TOWCommon.Say(agent.Name);
                    }
                    else
                    {
                        TOWCommon.Say("agent: common enemy " );
                    }
                }
            }
            
            foreach (var team in defender)
            {
                foreach (Agent agent in team.TeamAgents)
                {
                    if(agent.Character.IsPlayerCharacter||agent.Character.IsHero)
                        TOWCommon.Say(agent.Name);
                    
                    if (agent.IsHero||agent.IsMainAgent)
                    {
                        TOWCommon.Say("agent "+agent.Name);
                    }
                    else
                    {
                        TOWCommon.Say("agent: common enemy " );
                    }
                }
                
            }
        }
        
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            foreach(var agent in Mission.Current.AllAgents)
            {
                if (agent.GetComponent<StatusEffectComponent>() != null)
                {
                    if (agent.IsActive() && agent.Health > 0.1f)
                    {
                        var comp = agent.GetComponent<StatusEffectComponent>();
                        comp.OnTick(dt);
                    }
                }
            }
        }

        public override MissionBehaviourType BehaviourType { get; }
    }
    
    public class OnTickArgs : EventArgs
    {
        public float deltatime;
    }
}