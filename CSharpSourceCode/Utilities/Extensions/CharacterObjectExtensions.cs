using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.Battle.Damage;
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

        public static List<ResistanceTuple> GetDefenseProperties(this BasicCharacterObject characterObject)
        {
            var list = new List<ResistanceTuple>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId)?.Resistances;
            if (info != null)
            {
                list.AddRange(info);
            }
            return list;
        }
        
        public static List<AmplifierTuple> GetAttackProperties(this BasicCharacterObject characterObject)
        {
            var list = new List<AmplifierTuple>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId)?.DamageAmplifiers;
            if (info != null)
            {
                list.AddRange(info);
            }
            return list;
        }
        
        public static List<DamageProportionTuple> GetUnitDamageProportions(this BasicCharacterObject characterObject)
        {
            var list = new List<DamageProportionTuple>();
            var info = ExtendedInfoManager.GetCharacterInfoFor(characterObject.StringId)?.DamageProportions;
            if (info != null)
            {
                list.AddRange(info);
            }
            else
            {
                var defaultProp = new DamageProportionTuple(DamageType.Physical,1);
                list.Add(defaultProp);
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

        public static bool IsUndead(this BasicCharacterObject characterObject)
        {
            return characterObject.GetAttributes().Contains("Undead");
        }

        public static bool IsVampire(this BasicCharacterObject characterObject)
        {
            return characterObject.GetAttributes().Contains("VampireBodyOverride");
        }
        /// <summary>
        /// Access item objects from the equipment of the character
        /// Equipment Indexes can define the Range. Note that horses are not a valid item object to be accessed
        /// </summary>
        public static List<ItemObject> GetCharacterEquipment(this BasicCharacterObject characterObject,
            EquipmentIndex BeginningFrom=EquipmentIndex.Weapon0, EquipmentIndex EndingAt=EquipmentIndex.ArmorItemEndSlot)
        {
            int index = (int) BeginningFrom;
            int end = (int)EndingAt;
            List<ItemObject> CharacterEquipmentItems = new List<ItemObject>();
            for (int i = index; i <= end; i++)
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
