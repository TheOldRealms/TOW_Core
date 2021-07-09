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

        public override void AfterAddTeam(Team team)
        {
            base.AfterAddTeam(team);
            
           
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
            
        }
        
         public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            if (agent.IsMount)
                return;

            if (Campaign.Current == null)
            {
                
                if (agent.Team.IsAttacker)
                {
                    
                    if (agent== Mission.Current.MainAgent)
                    {
                        playerPartyAttribute = attackerAttributes[0];
                        AddStaticAttributeComponent(agent, attackerAttributes[0].RegularTroopAttributes[0], attackerAttributes[0]);
                    }
                    
                }
                else
                {
                    if (agent== Mission.Current.MainAgent)
                    {
                        playerPartyAttribute = defenderAttriubtes[0];
                        AddStaticAttributeComponent(agent, defenderAttriubtes[0].RegularTroopAttributes[0], defenderAttriubtes[0]);
                    }
                }
                
            }
            
            foreach (var partyAttribute in activeAttributes)
            {
                if (agent.Origin.BattleCombatant== partyAttribute.PartyBase)
                {
                    if (partyAttribute.PartyType == PartyType.RogueParty)
                    {
                        foreach (var attribute in partyAttribute.RegularTroopAttributes)
                        {
                            AddStaticAttributeComponent(agent, attribute, partyAttribute);
                            break;
                        }
                        break;
                    }

                    if (partyAttribute.PartyType == PartyType.Regular)
                    {
                        foreach (var attribute in partyAttribute.RegularTroopAttributes)
                        {
                            if (agent.Origin.Troop.ToString() == attribute.id)
                            {
                                AddStaticAttributeComponent(agent, attribute, partyAttribute);
                                break;
                            }
                        }
                        break;
                    }

                    if (partyAttribute.PartyType == PartyType.LordParty)
                    {
                        if (!agent.IsHero)
                        {
                            if (agent.Character.IsSoldier && !partyAttribute.RegularTroopAttributes.IsEmpty())
                            {
                                foreach (var attribute in partyAttribute.RegularTroopAttributes)
                                {
                                    if (agent.Character.ToString() == attribute.id)
                                    {
                                        AddStaticAttributeComponent(agent, attribute, partyAttribute);
                                        break;
                                    }

                                }
                                break;
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
                                foreach (var companionAttribute in partyAttribute.CompanionAttributes)
                                {
                                    if (agent.Character.Name.ToString() == companionAttribute.id)
                                    {
                                        AddStaticAttributeComponent(agent, companionAttribute, partyAttribute);
                                        break;
                                    }
                                }
                            }


                           
                        }
                    }
                }
                
            }
            _agents.Add(agent);

            if (agent == Mission.Current.MainAgent)
            {
                NotifyPlayerPartyAttributeAssignedObservers?.Invoke(); 
            }
        }
         
        private void AddStaticAttributeComponent(Agent agent, StaticAttribute attribute, PartyAttribute partyAttribute)
        {
            StaticAttributeAgentComponent agentComponent = new StaticAttributeAgentComponent(agent);
            agentComponent.SetParty(partyAttribute);
            agentComponent.SetAttribute(attribute);
            
            
           //agent.InitializePartyAttribute(partyAttribute);
            
            agent.AddComponent(agentComponent);
            
        }
        
        
        public delegate void OnPlayerPartyAttributeAssigned();
    }
}