using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.AttributeDataSystem
{
    /// <summary>
    /// Static Attribute attached to an agent on Battlefield
    /// </summary>
    public class AgentExtendedInfoComponent: AgentComponent
    {
        public AgentExtendedInfoComponent(Agent agent) : base(agent)
        {
            
        }
        
        private MobilePartyExtendedInfo _linkedPartyInfo;
        private CharacterExtendedInfo _characterInfo;
        
        public void SetAttribute(CharacterExtendedInfo info)
        {
            _characterInfo = info;
            foreach (var characterAttribute  in _characterInfo.CharacterAttributes)
            {
                this.Agent.AddAttribute(characterAttribute);
            }


            if (info.CharacterAttributes.Contains("AbilityUser"))
            {
                foreach (var ability in _characterInfo.Abilities)
                {
                    this.Agent.AddAbility(ability);
                }
            }
           


        }
        
        public void SetParty(MobilePartyExtendedInfo info)
        {
            _linkedPartyInfo = info; 
        }



        public MobilePartyExtendedInfo GetPartyInfo()
        {
            return _linkedPartyInfo;
        }
        
    }
}