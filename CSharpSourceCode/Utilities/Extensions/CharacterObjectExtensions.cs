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
            return characterObject.StringId.StartsWith("tor_");
        }

        public static bool IsTOWTemplate(this BasicCharacterObject characterObject)
        {
            return characterObject.StringId.StartsWith("tor_");
        }

        public static List<string> GetAbilities(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId);
            if(info != null)
            {
                list.AddRange(info.Abilities);
            }
            return list;
        }

        public static List<string> GetAttributes(this BasicCharacterObject characterObject)
        {
            var list = new List<string>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId);
            if (info != null)
            {
                list.AddRange(info.CharacterAttributes);
            }
            return list;
        }

        public static List<DefenseProperty> GetDefenseProperties(this BasicCharacterObject characterObject)
        {
            var list = new List<DefenseProperty>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId).Defense;
            if (info != null)
            {
                list.AddRange(info);
            }
            return list;
        }
        
        public static List<OffenseProperty> GetAttackProperties(this BasicCharacterObject characterObject)
        {
            var list = new List<OffenseProperty>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId).Offense;
            if (info != null)
            {
                list.AddRange(info);
            }
            return list;
        }
        
        

        public static string GetCustomVoiceClassName(this BasicCharacterObject characterObject)
        {
            string s = null;
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId);
            if (info != null)
            {
                s = info.VoiceClassName;
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

        public static List<ItemObject> GetCharacterEquipment(this BasicCharacterObject characterObject,
            EquipmentIndex BeginningFrom=EquipmentIndex.Weapon0)
        {
            List<ItemObject> CharacterEquipmentItems = new List<ItemObject>();
            for (int i = (int) BeginningFrom; i < 9; i++)
            {
                if (characterObject.Equipment[i].Item!=null)
                {
                    CharacterEquipmentItems.Add(characterObject.Equipment[i].Item);
                }
            }
            return CharacterEquipmentItems;
        }
    }
}
