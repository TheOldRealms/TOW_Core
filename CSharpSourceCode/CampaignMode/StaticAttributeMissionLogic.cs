using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignMode
{
    public class StaticAttributeMissionLogic : MissionLogic
    {
        private Mission.TeamCollection teams;
        private AttributeSystemManager _attributeSystemManager;

        private List<PartyAttribute> activeAttributes;
        
        private List<PartyAttribute> defenderAttributes;
        
        public override void AfterStart()
        {
            base.AfterStart();
            waitForTeamsAvaible();
            teams= Mission.Current.Teams;
            activeAttributes = new List<PartyAttribute>();
            defenderAttributes = new List<PartyAttribute>();
            
            if (Campaign.Current != null)
            {
                _attributeSystemManager = Campaign.Current.CampaignBehaviorManager.GetBehavior<AttributeSystemManager>();
                activeAttributes = _attributeSystemManager.GetActiveInvolvedParties();
            }
            
        }
        
        
        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            
            foreach (var partyAttribute in activeAttributes)
            {
                if (agent.Character != null)
                {
                    if ((agent.IsHero|| agent.IsPlayerControlled) 
                        && partyAttribute.LeaderAttribute!=null)
                    {
                        AddStaticAttributeComponent(agent, partyAttribute.LeaderAttribute);
                        break;
                    }
                    
                    if (!partyAttribute.CompanionAttributes.IsEmpty()&& agent.IsHero)
                    {
                        foreach (var companionAttribute in partyAttribute.CompanionAttributes)
                        {
                            if (agent.Character.Name.ToString() == companionAttribute.id)
                            {
                                AddStaticAttributeComponent(agent, companionAttribute);
                                break;
                            }

                        
                        }
                        break;
                    }

                }

                if (partyAttribute.RogueParty)
                {
                    foreach (var attribute in partyAttribute.RegularTroopAttributes)
                    {
                        if (agent.Origin.Troop.ToString() == attribute.id)
                        {
                            AddStaticAttributeComponent(agent,attribute);
                            break;
                        }
                    }
                    break;
                }
                
                if(agent.Character.IsSoldier)
                {
                    foreach (var attribute in partyAttribute.RegularTroopAttributes)
                    {
                        if(agent.Origin.Troop.ToString() == attribute.id)
                            AddStaticAttributeComponent(agent,attribute);
                        break;
                    }
                    break;
                }
               
            }
            
        }
        
        private async void waitForTeamsAvaible()
        {
            Task waitingForTeamsAvailable=   Task.Run(() => Mission.Current.Teams!=null);
            await waitingForTeamsAvailable.ConfigureAwait(false);
        }

        private async void waitForAssignedToTeam(Agent agent)
        {
            Task waitForAssignedToTeam = Task.Run(()=> agent.Team!=null);
            await waitForAssignedToTeam.ConfigureAwait(false);
        }
        

        private void AddStaticAttributeComponent(Agent agent, StaticAttribute attribute)
        {
            StaticAttributeAgentComponent agentComponent = new StaticAttributeAgentComponent(agent);
            agentComponent.SetAttribute(attribute);
            agent.AddComponent(agentComponent);
        }
    }
}