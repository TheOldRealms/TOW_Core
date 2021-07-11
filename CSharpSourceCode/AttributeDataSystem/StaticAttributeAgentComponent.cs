using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.AttributeDataSystem
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
            foreach (var characterAttribute  in _attribute.CharacterAttributes)
            {
                this.Agent.AddAttribute(characterAttribute);
            }


            if (attribute.IsMagicUser)
            {
                foreach (var ability in _attribute.Abilities)
                {
                    this.Agent.AddAbility(ability);
                }
            }
           


        }
        
        public void SetParty(PartyAttribute attribute)
        {
            //this.Agent.InitializePartyAttribute(attribute);
            _linkedPartyAttribute = attribute; 
        }



        public PartyAttribute GetParty()
        {
            return _linkedPartyAttribute;
        }
        
    }
}