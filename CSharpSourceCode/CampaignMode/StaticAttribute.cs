using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignMode
{
    public class StaticAttribute
    {
        [SaveableField(0)] public string race; // eg. undead, Vampire, Human
        [SaveableField(1)]public bool MagicUser;
        [SaveableField(2)] public int faith;
        [SaveableField(3)]public string status;
        [SaveableField(4)]public List<string> SkillBuffs =new List<string>();
        [SaveableField(5)] public List<string> MagicEffects = new List<string>();
    }
}