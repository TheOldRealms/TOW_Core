using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms.VisualStyles;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;
using TaleWorlds.TwoDimension;
using TOW_Core.Battle.Damage;
using TOW_Core.Battle.StatusEffects;
using TOW_Core.Items;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
using ItemObjectExtensions = TOW_Core.Utilities.Extensions.ItemObjectExtensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
        
        
        
        // we should decide, if we really want to override the damage type , if so we have to do some hierachical structuring

        private static DamageType DecideOnDominantEffectOverride(DamageType current, DamageType newType)
        {
            if (newType != DamageType.Physical)
            {
                if (current == DamageType.Fire && newType != DamageType.Magical)
                {
                    return current;
                }
                return newType;
            }
            return current;
        }
        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Agent), "HandleBlow")]
        public static bool PreHandleBlow(ref Blow b, ref Agent __instance)
        {
            //check for new Damage system, will change that 

            Agent attacker = b.OwnerId != -1 ? Mission.Current.FindAgentWithIndex(b.OwnerId) : __instance;
            Agent Victim = __instance;
            float ArmorPenetration=0f;
            float bonusDamage=0f;
            var resultDamageType = DamageType.Physical;

    
            // get attacker information
            if (!attacker.IsHero)
            {
                //unit attributes
                var offenseProperties = attacker.Character.GetAttackProperties();
                
                //add all offense properties
                foreach (var property in offenseProperties)
                {
                    ArmorPenetration += property.ArmorPenetration;
                    bonusDamage += property.BonusDamagePercent;
                    resultDamageType = property.DefaultDamageTypeOverride;       
                }
                
                //add temporary effects like buffs to attack bonuses
                List<ItemTrait> itemTraits = attacker.GetComponent<ItemTraitAgentComponent>()
                    .GetDynamicTraits(attacker.WieldedWeapon.Item);
                foreach (var temporaryTraits in itemTraits)
                {
                    if (temporaryTraits.OffenseProperty != null)
                    {
                        ArmorPenetration += temporaryTraits.OffenseProperty.ArmorPenetration;
                        bonusDamage += temporaryTraits.OffenseProperty.BonusDamagePercent;
                        resultDamageType = DecideOnDominantEffectOverride(resultDamageType, temporaryTraits.OffenseProperty.DefaultDamageTypeOverride);
                    }
                }
            }
            else
            {
                //Hero item level attributes 

                List<ItemTrait> itemTraits= new List<ItemTrait>();
                List<ItemObject> items;
                ItemDamageProperty damageProperty;
                
                // get all equipment Pieces

                items = attacker.Character.GetCharacterEquipment();
                foreach (var item in items)
                {
                    if(item.HasTrait())
                        itemTraits.AddRange(item.GetTraits());
                    
                    itemTraits.AddRange(attacker.GetComponent<ItemTraitAgentComponent>().GetDynamicTraits(item));
                }

                foreach (var itemTrait in itemTraits)
                {
                    if(itemTrait.OffenseProperty==null) 
                        continue;
                    
                    ArmorPenetration += itemTrait.OffenseProperty.ArmorPenetration;
                    bonusDamage += itemTrait.OffenseProperty.BonusDamagePercent;
                    
                    //TODO i am still not so sure if we should have a Override, in that form. 

                    resultDamageType = DecideOnDominantEffectOverride(resultDamageType, itemTrait.OffenseProperty.DefaultDamageTypeOverride);
                }
                
                //weapon properties
                var damageProperties = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                if (damageProperties != null)
                {
                    resultDamageType = DecideOnDominantEffectOverride(resultDamageType, damageProperties.DamageType);
                       // maximum minimum is really not needed. just go with objects.xml in the prenative module do it here anywhere
                    Mathf.Clamp(b.InflictedDamage, damageProperties.MinDamage, damageProperties.MaxDamage);
                }
            }
            
            
            
            
            



            return true;

        }
    }
}