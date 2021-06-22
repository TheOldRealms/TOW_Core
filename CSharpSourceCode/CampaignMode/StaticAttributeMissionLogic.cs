using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignMode
{
    public class StaticAttributeMissionLogic : MissionLogic
    {
        private Mission.TeamCollection teams;
        private AttributeSystemManager _attributeSystemManager;

        private List<PartyAttribute> activeAttributes;
        
        private List<PartyAttribute> defenderAttributes;

        private List<Agent> _agents;
        
        public override void AfterStart()
        {
            base.AfterStart();
            waitForTeamsAvaible();
            teams= Mission.Current.Teams;
            activeAttributes = new List<PartyAttribute>();
            defenderAttributes = new List<PartyAttribute>();
            _agents = new List<Agent>();
            
            if (Campaign.Current != null)
            {
                _attributeSystemManager= AttributeSystemManager.Instance;
                  //  _attributeSystemManager = Campaign.Current.CampaignBehaviorManager.GetBehavior<AttributeSystemManager>();
                activeAttributes = _attributeSystemManager.GetActiveInvolvedParties();
            }
            
        }

        public override void AfterAddTeam(Team team)
        {
            base.AfterAddTeam(team);
            TOWCommon.Say("added Team to dictionary ");
        }

         public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            if (agent.IsMount)
                return;
            
            
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
           // TOWCommon.Say("agent of " +  agent.Origin.BattleCombatant.Name.ToString()+  "added to Banner to dictionary ");
        }

        public override void OnCreated()
        {
            base.OnCreated();
            TOWCommon.Say("created  ");
        }

        public override void OnAgentCreated(Agent agent)
        {
            /*if (Mission.Current.IsFriendlyMission)
            {
                return;
            }
            
            base.OnAgentCreated(agent);
            
            
            //var party = agent.GetComponent<CampaignAgentComponent>().OwnerParty; // this would save a lot of trouble but dont work
            //PartyAttribute PartyAttribute = _attributeSystemManager.GetAttribute(party.Id);
            

            if (agent.IsMount)
                return;
            
            List<PartyAttribute> partyAttributes = activeAttributes;
            
            foreach (var partyAttribute in partyAttributes)
            {
                if (agent.Character == null) continue;
                
                if (partyAttribute.PartyType == PartyType.RogueParty && !partyAttribute.RegularTroopAttributes.IsEmpty())
                {
                    if (!agent.Character.IsSoldier && !agent.Character.IsHero)      //find a better way to determine if agent is a rogue 
                    {
                        foreach (var attribute in partyAttribute.RegularTroopAttributes)        
                        {
                            AddStaticAttributeComponent(agent,attribute, partyAttribute);
                            break;
                        }
                            
                    }
                    continue;
                }
                
                if (partyAttribute.PartyType == PartyType.Regular &&
                    partyAttribute.RegularTroopAttributes.IsEmpty())
                {
                    if (agent.Character.IsSoldier && !agent.Character.IsHero)
                    {
                        foreach (var attribute in partyAttribute.RegularTroopAttributes)
                        {
                            if (agent.Character.ToString() == attribute.id)
                            {
                                AddStaticAttributeComponent(agent, attribute, partyAttribute);
                                break;
                            }

                        }
                        continue;
                    }
                }

                if (partyAttribute.PartyType != PartyType.LordParty || partyAttribute.Leader == null) continue;

                if (!agent.IsHero)
                {
                    if (agent.Character.IsSoldier && partyAttribute.RegularTroopAttributes.IsEmpty())
                    {
                        foreach (var attribute in partyAttribute.RegularTroopAttributes)
                        {
                            if (agent.Character.ToString() == attribute.id)
                            {
                                AddStaticAttributeComponent(agent, attribute, partyAttribute);
                                break;
                            }

                        }
                        continue;
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
            _agents.Add(agent);
            TOWCommon.Say("added agent" + agent.Name +"to dictionary");*/
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
        

        private void AddStaticAttributeComponent(Agent agent, StaticAttribute attribute, PartyAttribute partyAttribute)
        {
            StaticAttributeAgentComponent agentComponent = new StaticAttributeAgentComponent(agent);
            agentComponent.SetAttribute(attribute);
            agentComponent.SetParty(partyAttribute);
            agent.AddComponent(agentComponent);
        }
    }
}