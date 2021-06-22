using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignMode
{
    /// <summary>
    /// Contains Tow data of single unit or character. For attributes affecting whole Party use a PartyAttribute
    /// </summary>
    public class StaticAttribute
    {
        //dummy data
        [SaveableField(0)] public string race; // eg. undead, Vampire, Human
        [SaveableField(1)]public bool MagicUser;
        [SaveableField(2)] public int faith;
        [SaveableField(3)]public string status;
        [SaveableField(4)]public List<string> SkillBuffs =new List<string>();
        [SaveableField(5)]public List<string> MagicEffects = new List<string>();
        [SaveableField(6)]public string id;
        [SaveableField(7)]public int number;
        [SaveableField(8)]public PartyBase AssignedParty;
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