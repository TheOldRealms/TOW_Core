using TaleWorlds.MountAndBlade;

namespace TOW_Core.CampaignMode
{
    public class StaticAttributeAgentComponent: AgentComponent
    {
        private PartyAttribute _linkedPartyAttribute;
        private StaticAttribute _attribute;
        public StaticAttributeAgentComponent(Agent agent) : base(agent)
        {
            
        }


        public void SetAttribute(StaticAttribute attribute)
        {
            _attribute = attribute; 
        }
    }
}