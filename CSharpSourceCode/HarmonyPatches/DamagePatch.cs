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
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
using ItemObjectExtensions = TOW_Core.Utilities.Extensions.ItemObjectExtensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
        
        // helper function

        private static DamageType DecideOnDominantDamageEffectOverride(DamageType current, DamageType newType)
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
            Agent attacker = b.OwnerId != -1 ? Mission.Current.FindAgentWithIndex(b.OwnerId) : __instance;
            Agent Victim = __instance;
            
            //attack
            float ArmorPenetration=0f;
            float bonusDamage=0f;
            var resultDamageType = DamageType.Physical;
            
            //defense
            float damageReduction = 0f;
            var DamageResistanceType = DamageType.Physical;
            DefenseType DefenseType = DefenseType.None;


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
                        resultDamageType = DecideOnDominantDamageEffectOverride(resultDamageType, temporaryTraits.OffenseProperty.DefaultDamageTypeOverride);
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

                items = attacker.Character.GetCharacterEquipment(EquipmentIndex.ArmorItemBeginSlot);
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

                    resultDamageType = DecideOnDominantDamageEffectOverride(resultDamageType, itemTrait.OffenseProperty.DefaultDamageTypeOverride);
                }
                
                //weapon properties
                var damageProperties = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                if (damageProperties != null)
                {
                    resultDamageType = DecideOnDominantDamageEffectOverride(resultDamageType, damageProperties.DamageType);
                       // maximum minimum is really not needed anymore. just go with objects.xml in the prenative module do it here anywhere
                    Mathf.Clamp(b.InflictedDamage, damageProperties.MinDamage, damageProperties.MaxDamage);
                }
            }
            
            //defender information

            if (!Victim.IsHero)
            {
                //unit attributes
                var defenseProperties = attacker.Character.GetDefenseProperties();
                
                //add all defense properties from unit properties
                foreach (var property in defenseProperties)
                {
                    damageReduction += property.ReductionPercent;
                    DamageResistanceType = property.ResistedDamageType;      // do not know how to act on this probably Bitmask for Damage and Resistance is imo much cleaner. 
                }
                
            }
            else
            {
                //Hero item level attributes 

                List<ItemTrait> itemTraits= new List<ItemTrait>();
                List<ItemObject> items;
                ItemDamageProperty damageProperty;
                
                // get all equipment Pieces

                items = attacker.Character.GetCharacterEquipment(EquipmentIndex.ArmorItemBeginSlot);
                foreach (var item in items)
                {
                    if(item.HasTrait())
                        itemTraits.AddRange(item.GetTraits());
                    
                    itemTraits.AddRange(attacker.GetComponent<ItemTraitAgentComponent>().GetDynamicTraits(item));
                }

                foreach (var itemTrait in itemTraits)
                {
                    if(itemTrait.DefenseProperty==null) 
                        continue;
                    
                    damageReduction += itemTrait.DefenseProperty.ReductionPercent;
                }
                
                //weapon properties
                var damageProperties = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                if (damageProperties != null)
                {
                    resultDamageType = DecideOnDominantDamageEffectOverride(resultDamageType, damageProperties.DamageType);
                    // maximum minimum is really not needed anymore. just go with objects.xml in the prenative module do it here anywhere
                    Mathf.Clamp(b.InflictedDamage, damageProperties.MinDamage, damageProperties.MaxDamage);
                }
                
            }
            
            
            //reading out Status Effect Component of Victim (FOR debuffs and buffs on the Unit, not weapon related) do we want to have damage enhancing status effects? for what do we need dynamic item properties then?
            StatusEffectComponent.EffectAggregate effectAggregate = Victim.GetComponent<StatusEffectComponent>().GetEffectAggregate();
            damageReduction += effectAggregate.PercentageArmorEffect;
            
            
            
            
            // final calculation
            
            //all damage types and their values were added ( e.g. Physical, Fire, Death, Shadow...  )  
            // all defense values were added 
            // all damage and defense values were subtracted from each other by pairs, final numbers and were added together
            // negative values decrease the inflicted damage , positive increase
            // damage amplification percentage - reduction percentage
            // if positive, damage is amplified by %
            // if negative damage is shrinked by %

            // calculation for resistances and damag
            b.InflictedDamage =(int) (b.InflictedDamage + bonusDamage - damageReduction);
            

            
            return true;

        }
    }
}