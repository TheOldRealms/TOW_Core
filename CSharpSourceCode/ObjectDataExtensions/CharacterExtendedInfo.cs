using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Contains Tow data of single unit or character template. 
    /// </summary>
    public class CharacterExtendedInfo
    {
        public string CharacterStringId;
        public List<string> Abilities = new List<string>();
        public List<string> CharacterAttributes = new List<string>();
        public string VoiceClassName;
    }
}