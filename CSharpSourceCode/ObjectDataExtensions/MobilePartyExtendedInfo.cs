using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Contains TOW troop data of a complete mobile Party. For single entity data use a StaticAttribute
    /// </summary>
    public class MobilePartyExtendedInfo
    {
        [SaveableField(0)] public string PartyBaseId;
        [SaveableField(1)] public Hero Leader;
        [SaveableField(3)] public HeroExtendedInfo LeaderInfo;
        [SaveableField(4)] public PartyType PartyType;
        [SaveableField(5)] public PartyBase PartyBase;
        
        public MobilePartyExtendedInfo(string partyBaseId)
        {
            this.PartyBaseId = partyBaseId;
        }

        public MobilePartyExtendedInfo()
        {

        }        
    }
    
    
    public class PartyAttributeDefiner : SaveableTypeDefiner
    {
        public PartyAttributeDefiner() : base(1_543_132) { }
        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(MobilePartyExtendedInfo), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(Dictionary<string, MobilePartyExtendedInfo>));
        }
    }
    
    
    public enum PartyType{
        [SaveableField(1)]BanditParty,
        [SaveableField(2)]LordParty,
        [SaveableField(3)]Regular,
    }

    public class PartyTypeDefiner : SaveableTypeDefiner
    {
        public PartyTypeDefiner() : base(1_543_134)
        {
            
        }
        protected override void DefineEnumTypes()
        {
            base.DefineEnumTypes();
            AddEnumDefinition(typeof(PartyType), 1);
        }
        
    }
}