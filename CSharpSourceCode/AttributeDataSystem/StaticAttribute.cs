using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.AttributeDataSystem
{
    /// <summary>
    /// Contains Tow data of single unit or character. For attributes affecting whole Party use a PartyAttribute
    /// </summary>
    public class StaticAttribute
    {
        //dummy data types
        [SaveableField(0)] private  bool _isMagicUser;   
        [SaveableField(1)] public int faith;    
        [SaveableField(2)]public string status;
        [SaveableField(3)]public List<string> Abilities =new List<string>();
        [SaveableField(4)]public List<string> CharacterAttributes = new List<string>();
        [SaveableField(5)]public string id;
        [SaveableField(6)]public int number;
        [SaveableField(7)]public PartyBase AssignedParty;
        [SaveableField(8)]public PartyAttribute AssignedPartyAttribute;


        public StaticAttribute (PartyAttribute partyAttribute)
        {
            AssignedPartyAttribute = partyAttribute;
        }

        public bool IsMagicUser
        {
            get
            { 
               return  _isMagicUser;
            }
            set
            {
                _isMagicUser = value;
                AssignedPartyAttribute.MagicUserStateChanged();
            }
        }

        
        
    }
    public class StaticAttributeDefiner : SaveableTypeDefiner
    {
        public StaticAttributeDefiner() : base(1_543_133) { }
        protected override void DefineClassTypes()
        {
            base.DefineClassTypes();
            AddClassDefinition(typeof(StaticAttribute), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            base.DefineContainerDefinitions();
            ConstructContainerDefinition(typeof(List<StaticAttribute>));
        }
    }
}