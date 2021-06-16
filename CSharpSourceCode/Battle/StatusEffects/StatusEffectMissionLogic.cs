using System;
using System.Collections.Generic;
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
            _attributeSystemManager = Campaign.Current.CampaignBehaviorManager
                .GetBehavior<AttributeSystemManager>();
            attackerAttributes = _attributeSystemManager.GetActiveAttackerAttributes();
            defenderAttributes = _attributeSystemManager.GetActiveDefenderAttributes();
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

        public override void AfterAddTeam(Team team)
        {
            base.AfterAddTeam(team);
            if (team.IsAttacker)
            {
                foreach (var party in attackerAttributes)
                {
                    if (party.MagicUserParty)
                    {
                        TOWCommon.Say(party.id +" are magic users and have "+ party.WindsOfMagic+" Winds of magic available");
                    }
                }
            }

            if (team.IsDefender)
            {
                foreach (var party in defenderAttributes)
                {
                    if (party.MagicUserParty)
                    {
                        TOWCommon.Say(party.id +" are magic users and have "+ party.WindsOfMagic+" Winds of magic available");
                    }
                }
            }
        }

        /*public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            
            /*if (agent.IsHero)
            {
                TOWCommon.Say(defenderAttributes[0].WindsOfMagic.ToString());
            }#1#
        }*/

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