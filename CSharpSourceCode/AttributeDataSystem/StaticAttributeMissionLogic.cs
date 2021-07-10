using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.AttributeDataSystem
{
    /// <summary>
    /// Reads out PartyAttributeManager during Combat
    /// </summary>
    public class StaticAttributeMissionLogic : MissionLogic
    {
        private Mission.TeamCollection teams;
        private PartyAttributeManager _partyAttributeManager;

        private List<PartyAttribute> activeAttributes;
        

        private List<Agent> _agents;


        private List<PartyAttribute> attackerAttributes;
        private List<PartyAttribute> defenderAttriubtes;

        private PartyAttribute playerPartyAttribute;

        private bool _isCustomBattle;

        public event OnPlayerPartyAttributeAssigned NotifyPlayerPartyAttributeAssignedObservers;

        public List<PartyAttribute> GetAttackerAttributes()
        {
            return attackerAttributes;
        }
        
        public List<PartyAttribute> GetDefenderAttributes()
        {
            return defenderAttriubtes;
        }

        public PartyAttribute GetPlayerAttribute()
        {
            return playerPartyAttribute;
        }
        
        public override void AfterStart()
        {
            base.AfterStart();
            teams= Mission.Current.Teams;
            activeAttributes = new List<PartyAttribute>();
            _agents = new List<Agent>();
            attackerAttributes = new List<PartyAttribute>();
            defenderAttriubtes = new List<PartyAttribute>();
            if (Campaign.Current != null)
            {
                _partyAttributeManager = Campaign.Current.GetCampaignBehavior<PartyAttributeManager>();
                activeAttributes = _partyAttributeManager.GetActiveInvolvedParties();
            }
            else
            {
                SetForCustomBattle();
                _isCustomBattle = true;
            }
            
        }
        
         public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            if (agent.IsMount)
                return;

            if (_isCustomBattle)
            {
                
                if (agent.Team.IsAttacker)
                {
                    if (agent== Mission.Current.MainAgent)
                    {
                        playerPartyAttribute = attackerAttributes[0];
                    }
                    AddStaticAttributeComponent(agent, attackerAttributes[0].RegularTroopAttributes[0], attackerAttributes[0]);
                }
                else
                {
                    if (agent== Mission.Current.MainAgent)
                    {
                        playerPartyAttribute = attackerAttributes[0];
                        
                    }
                    
                    AddStaticAttributeComponent(agent, defenderAttriubtes[0].RegularTroopAttributes[0], defenderAttriubtes[0]);
                }
                
            }
            else
            {
                foreach (var partyAttribute in activeAttributes)
                {
                    if (agent.Origin.BattleCombatant== partyAttribute.PartyBase)
                    {
                        var partyType = partyAttribute.PartyType;
                        switch (partyType)
                        {
                            case PartyType.RogueParty:
                                AddStaticAttributeComponent(agent,partyAttribute.RegularTroopAttributes[0],partyAttribute);
                                break;
                            
                            case PartyType.Regular:
                                var regularTroopAttribute = FindAttribute(agent.Origin.Troop.ToString(), partyAttribute.RegularTroopAttributes);
                                if (regularTroopAttribute != null)
                                    AddStaticAttributeComponent(agent, regularTroopAttribute, partyAttribute);
                                break;
                            
                            case PartyType.LordParty:
                                if (!agent.IsHero)
                                {
                                    if (agent.Character.IsSoldier && !partyAttribute.RegularTroopAttributes.IsEmpty())
                                    {
                                        var LordPartyRegularTroopAttribute = FindAttribute(agent.Origin.Troop.ToString(), partyAttribute.RegularTroopAttributes);
                                        if (LordPartyRegularTroopAttribute != null)
                                            AddStaticAttributeComponent(agent, LordPartyRegularTroopAttribute, partyAttribute);
                                    }
                                }
                                else
                                {
                                    if (agent.Character.Name == partyAttribute.Leader.Name)
                                    {
                                        var leaderAttribute = partyAttribute.LeaderAttribute;
                                        AddStaticAttributeComponent(agent, leaderAttribute, partyAttribute);
                                        break;
                                    }
                                    if (!partyAttribute.CompanionAttributes.IsEmpty())
                                    {
                                        var CompanionAttribute = FindAttribute(agent.Character.Name.ToString(), partyAttribute.RegularTroopAttributes);
                                        if (CompanionAttribute != null)
                                            AddStaticAttributeComponent(agent, CompanionAttribute, partyAttribute);
                                    }
                                }
                                break;
                        }
                        
                    }
                }
            
            
                
            }
            _agents.Add(agent);

            
        }


         private void SetForCustomBattle()
         {
             activeAttributes = new List<PartyAttribute>();
             PartyAttribute attackerAttribute = new PartyAttribute();
             attackerAttribute.PartyBaseId = "attacker";
             attackerAttributes.Add(attackerAttribute);
             PartyAttribute defenderAttribute = new PartyAttribute();
             defenderAttribute.PartyBaseId = "defender";
             defenderAttriubtes.Add(defenderAttribute);
             activeAttributes.Add(attackerAttribute);
             activeAttributes.Add(defenderAttribute);
             foreach (var partyAttribute in activeAttributes)
             {
                 StaticAttribute standardAttribute = new StaticAttribute(partyAttribute);
                 partyAttribute.RegularTroopAttributes.Add(standardAttribute);
                 partyAttribute.WindsOfMagic = 30f;
                 partyAttribute.IsMagicUserParty = true;
             }
         }

         private StaticAttribute FindAttribute(string id, List<StaticAttribute> attributes)
         {
             foreach (var attribute in attributes)
             {
                 if (id == attribute.id)
                 {
                     return attribute;
                 }
             }

             return null;
         }
        
         
        private void AddStaticAttributeComponent(Agent agent, StaticAttribute attribute, PartyAttribute partyAttribute)
        {
            StaticAttributeAgentComponent agentComponent = new StaticAttributeAgentComponent(agent);
            agentComponent.SetParty(partyAttribute);
            agentComponent.SetAttribute(attribute);
            
            
           //agent.InitializePartyAttribute(partyAttribute);
            
            agent.AddComponent(agentComponent);
            
            if (agent == Mission.Current.MainAgent)
            {
                playerPartyAttribute = partyAttribute;
                NotifyPlayerPartyAttributeAssignedObservers?.Invoke(); 
            }
            
        }
        
        
        public delegate void OnPlayerPartyAttributeAssigned();
    }
}