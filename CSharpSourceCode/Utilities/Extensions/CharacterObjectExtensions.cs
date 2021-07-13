using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Utilities.Extensions
{
    public static class CharacterObjectExtensions
    {
        public static bool IsTOWTemplate(this CharacterObject characterObject)
        {
            return characterObject.StringId.Contains("tow_");
        }

        public static List<string> GetAbilities(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            var infoManager = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if(infoManager != null)
            {
                var info = infoManager.GetCharacterInfoFor(characterObject.StringId);
                foreach(var item in info.Abilities)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static List<string> GetAttributes(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            var infoManager = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if (infoManager != null)
            {
                var info = infoManager.GetCharacterInfoFor(characterObject.StringId);
                foreach (var item in info.CharacterAttributes)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static string GetCustomVoiceClassName(this BasicCharacterObject characterObject)
        {
            string s = null;
            var infoManager = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if (infoManager != null)
            {
                var info = infoManager.GetCharacterInfoFor(characterObject.StringId);
                s = info.VoiceClassName;
            }
            return s;
        }
    }
}
