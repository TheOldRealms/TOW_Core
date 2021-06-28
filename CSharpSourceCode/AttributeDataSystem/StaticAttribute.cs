using System.Collections.Generic;
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
        [SaveableField(0)] public string race;  //mostly hardcoded currently needs to be assigned via xml or similar also counts for many other values in this class
        [SaveableField(1)] public string culture; // eg. undead, Vampire, Human
        [SaveableField(2)]public bool IsMagicUser;   
        [SaveableField(3)] public int faith;    
        [SaveableField(4)]public string status;
        [SaveableField(5)]public List<string> SkillBuffs =new List<string>();
        [SaveableField(6)]public List<string> MagicEffects = new List<string>();
        [SaveableField(7)]public string id;
        [SaveableField(8)]public int number;
        [SaveableField(9)]public PartyBase AssignedParty;
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