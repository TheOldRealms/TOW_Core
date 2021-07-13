using TaleWorlds.MountAndBlade;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Extended information about an agent on Battlefield/Mission
    /// </summary>
    public class AgentExtendedInfoComponent: AgentComponent
    {
        private MobilePartyExtendedInfo _linkedPartyInfo;
        private HeroExtendedInfo _heroInfo;
        public AgentExtendedInfoComponent(Agent agent) : base(agent) { }
        
        public void SetParty(MobilePartyExtendedInfo info) => _linkedPartyInfo = info; 
        public void SetHeroInfo(HeroExtendedInfo info) => _heroInfo = info;
        public MobilePartyExtendedInfo GetPartyInfo() => _linkedPartyInfo;
        public HeroExtendedInfo GetHeroInfo() => _heroInfo;        
    }
}