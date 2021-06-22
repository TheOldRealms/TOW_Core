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
    /// <summary>
    /// Reads out PartyAttributeManager during Combat
    /// </summary>
    public class StaticAttributeMissionLogic : MissionLogic
    {
        private Mission.TeamCollection teams;
        private PartyAttributeManager _partyAttributeManager;

        private List<PartyAttribute> activeAttributes;
        
        private List<PartyAttribute> defenderAttributes;

        private List<Agent> _agents;
        
        public override void AfterStart()
        {
            base.AfterStart();
            teams= Mission.Current.Teams;
            activeAttributes = new List<PartyAttribute>();
            defenderAttributes = new List<PartyAttribute>();
            _agents = new List<Agent>();
            
            if (Campaign.Current != null)
            {
                _partyAttributeManager= PartyAttributeManager.Instance;
                activeAttributes = _partyAttributeManager.GetActiveInvolvedParties();
            }
            
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
         
        private void AddStaticAttributeComponent(Agent agent, StaticAttribute attribute, PartyAttribute partyAttribute)
        {
            StaticAttributeAgentComponent agentComponent = new StaticAttributeAgentComponent(agent);
            agentComponent.SetAttribute(attribute);
            agentComponent.SetParty(partyAttribute);
            agent.AddComponent(agentComponent);
        }
    }
}