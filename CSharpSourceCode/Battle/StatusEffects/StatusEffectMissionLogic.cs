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
using TOW_Core.Battle.Extensions;
using TOW_Core.CampaignMode;
using TOW_Core.Utilities;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectMissionLogic : MissionLogic
    {
        private Dictionary<string, StatusEffect> _presentEffects = new Dictionary<string, StatusEffect>();

        public EventHandler<OnTickArgs> NotifyStatusEffectTickObservers;

        private AttributeSystemManager _attributeSystemManager;

        private List<PartyAttribute> attackerAttributes;
        
        private List<PartyAttribute> defenderAttributes;


        public override void AfterStart()
        {
            base.AfterStart();
            attackerAttributes = new List<PartyAttribute>();
            defenderAttributes = new List<PartyAttribute>();
            
            if (Campaign.Current != null)
            {
                _attributeSystemManager = Campaign.Current.CampaignBehaviorManager
                    .GetBehavior<AttributeSystemManager>();
                attackerAttributes = _attributeSystemManager.GetActiveAttackerAttributes();
                defenderAttributes = _attributeSystemManager.GetActiveDefenderAttributes();
                
               
            }

            
                
        }
        
        

        
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            StatusEffectComponent effectComponent = new StatusEffectComponent(agent);
            NotifyStatusEffectTickObservers += effectComponent.OnTick;
            agent.AddComponent(effectComponent);
            if(!_presentEffects.ContainsKey("crumble") && agent.IsUndead())
            {
                _presentEffects.Add("crumble", StatusEffectManager.GetStatusEffect("crumble"));
            }

            /*if (agent.Character.IsHero)
            {
                TOWCommon.Say(defenderAttributes[0].WindsOfMagic.ToString());
            }*/
        }


        public override void OnAfterMissionCreated()
        {
            base.OnAfterMissionCreated();

            //var attacker = Mission.Current.Teams.Attacker;
            
            var attacker = Mission.Current.Teams.GetAlliesOf(Mission.Current.Teams.Attacker, true);
            var defender = Mission.Current.Teams.GetAlliesOf(Mission.Current.Teams.Defender, true);
            
            var agents =Mission.Current.Agents;
            
            
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

                foreach (var attribute in attackerAttributes)
                {
                    if(attribute.Leader!=null)
                        TOWCommon.Say(attribute.Leader.Name.ToString());
                    else
                    {
                        TOWCommon.Say("part of gang: " + attribute.id+  " ("+attackerAttributes.Count+")");
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

                foreach (var attribute in defenderAttributes)
                {
                    if(attribute.Leader!=null)
                        TOWCommon.Say("attribute "+ attribute.Leader.Name.ToString());
                    else
                    {
                        TOWCommon.Say("part of gang: " + attribute.id+  " ("+defenderAttributes.Count+")");
                    }
                }

                
            }
        }

        public override void AfterAddTeam(Team team)
        {
            base.AfterAddTeam(team);
            
        }

        /*public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            
            /*if (agent.IsHero)
            {
                TOWCommon.Say(defenderAttributes[0].WindsOfMagic.ToString());
            }#1#
        }*/

        public override void OnRenderingStarted()
        {
            
        }

        public override void OnCreated()
        {
            
            
            
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            OnTickArgs arguments = new OnTickArgs() { deltatime = dt };
            NotifyStatusEffectTickObservers?.Invoke(this, arguments);
        }

        public override MissionBehaviourType BehaviourType { get; }
    }



    public class OnTickArgs : EventArgs
    {
        public float deltatime;
    }
}