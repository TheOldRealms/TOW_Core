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
            return characterObject.StringId.StartsWith("tow_");
        }

        public static bool IsTOWTemplate(this BasicCharacterObject characterObject)
        {
            return characterObject.StringId.StartsWith("tow_");
        }

        public static List<string> GetAbilities(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            var infoManager = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if(infoManager != null)
            {
                var info = infoManager.GetCharacterInfoFor(characterObject.StringId);
                if(info != null && info.Abilities != null)
                {
                    list.AddRange(info.Abilities);
                }
            }
            else
            {
                var info = ExtendedInfoManager.GetCharacterInfoForStatic(characterObject.StringId);
                if (info != null && info.Abilities != null)
                {
                    list.AddRange(info.Abilities);
                }
            }
            return list;
        }

        public static List<string> GetAttributes(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            if (characterObject == null) return list;
            var infoManager = Campaign.Current?.CampaignBehaviorManager?.GetBehavior<ExtendedInfoManager>();
            if (infoManager != null)
            {
                var info = infoManager.GetCharacterInfoFor(characterObject.StringId);
                if(info != null && info.CharacterAttributes != null)
                {
                    list.AddRange(info.CharacterAttributes);
                }
            }
            else
            {
                var info = ExtendedInfoManager.GetCharacterInfoForStatic(characterObject.StringId);
                if (info != null && info.CharacterAttributes != null)
                {
                    list.AddRange(info.CharacterAttributes);
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
                if(info != null) s = info.VoiceClassName;
            }
            else
            {
                var info = ExtendedInfoManager.GetCharacterInfoForStatic(characterObject.StringId);
                if (info != null && info.VoiceClassName != null)
                {
                    s = info.VoiceClassName;
                }
            }
            return s;
        }

        public static bool IsUndead(this CharacterObject characterObject)
        {
            if (characterObject.IsHero)
            {
                return characterObject.HeroObject.IsUndead();
            }
            return characterObject.GetAttributes().Contains("Undead");
        }

        public static bool IsVampire(this CharacterObject characterObject)
        {
            if (characterObject.IsHero)
            {
                return characterObject.HeroObject.IsVampire();
            }
            return characterObject.GetAttributes().Contains("VampireBodyOverride");
        }
    }
}
