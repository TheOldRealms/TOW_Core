using TaleWorlds.MountAndBlade;

namespace TOW_Core.CampaignMode
{
    /// <summary>
    /// Static Attribute attached to an agent on Battlefield
    /// </summary>
    public class StaticAttributeAgentComponent: AgentComponent
    {
        
        
        public StaticAttributeAgentComponent(Agent agent) : base(agent)
        {
            
        }
        
        private PartyAttribute _linkedPartyAttribute;
        private StaticAttribute _attribute;
        
        public void SetAttribute(StaticAttribute attribute)
        {
            _attribute = attribute; 
        }
        
        public void SetParty(PartyAttribute attribute)
        {
            _linkedPartyAttribute = attribute; 
        }
        
    }
}