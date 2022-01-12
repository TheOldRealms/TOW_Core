using System;
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
        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Agent), "HandleBlow")]
        public static bool PreHandleBlow(ref Blow b, ref Agent __instance)
        {

            Agent attacker = b.OwnerId != -1 ? Mission.Current.FindAgentWithIndex(b.OwnerId) : __instance;
            Agent Victim = __instance;


            if (Victim.IsMount || attacker.IsMount)
            {
                return true;
            }


                //currently unused
            float Convertion=0.5f;
            float damageAmplification=0.0f;
            float damageReduction = 0f;
            
            
            //attack
            float armorPenetration=0f;
            
            var targetDamageType = DamageType.Physical;
            
            float[] damagePercentages = new float[5];
            
            

            DamageType additionalDamageType = DamageType.Physical;
            
            
           
             if (!attacker.IsHero)
             {
                 //unit attributes
                 var offenseProperties = attacker.Character.GetAttackProperties();
                 DamageType overrideDamage;
                
                 //add all offense properties
                 foreach (var property in offenseProperties)
                 {
                     armorPenetration += property.ArmorPenetration;
                     damagePercentages[(int) property.DefaultDamageTypeOverride] += property.BonusDamagePercent;
                 }
                
                 //add temporary effects like buffs to attack bonuses
                 List<ItemTrait> itemTraits = attacker.GetComponent<ItemTraitAgentComponent>()
                     .GetDynamicTraits(attacker.WieldedWeapon.Item);
                 foreach (var temporaryTraits in itemTraits)
                 {
                     var attackProperty = temporaryTraits.OffenseProperty;
                     if (attackProperty != null)
                     {
                         armorPenetration += attackProperty.ArmorPenetration;
                         damagePercentages[(int) temporaryTraits.OffenseProperty.DefaultDamageTypeOverride] += attackProperty.BonusDamagePercent;
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
                     var property = itemTrait.OffenseProperty;
                     if(property==null) 
                         continue;
                     armorPenetration += property.ArmorPenetration;
                     damagePercentages[(int) property.DefaultDamageTypeOverride] += property.BonusDamagePercent;
                    
                 }
                
                 //weapon properties
                 var weaponProperty = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                 if (weaponProperty != null)
                 {
                     damagePercentages[(int)weaponProperty.DamageType] += 0; //should provide some additional damage maybe?
                   
                 }
             }
            
            // suggestion: the damage type with the most % (without physical) is selected as resulting additional damage type
            additionalDamageType = DamageType.Physical; 
            var maximum = 0f;
            for (int i = 1; i < 5; i++)
            {
                if (maximum < damagePercentages[i])
                {
                    maximum = damagePercentages[i];
                    additionalDamageType = (DamageType) i;
                }
            }
            
            //defender information
            

           
                //armor for penetration otherwise no use since the physical calculation has already taken place.
            var victimEquipment = Victim.Character.Equipment;
            float armor = 0f;
            var bodyPart = b.VictimBodyPart;
            switch (bodyPart)
            {
                case BoneBodyPartType.Abdomen:
                case BoneBodyPartType.Chest:
                    armor += victimEquipment.GetHumanBodyArmorSum();
                    break;
                case BoneBodyPartType.Head:
                case BoneBodyPartType.Neck:
                    armor += victimEquipment.GetHeadArmorSum();
                    break;
                case BoneBodyPartType.Legs:
                    armor += victimEquipment.GetLegArmorSum();
                    break;
                case BoneBodyPartType.ArmLeft:
                case BoneBodyPartType.ShoulderLeft:
                case BoneBodyPartType.ArmRight:
                case BoneBodyPartType.ShoulderRight:
                    armor += victimEquipment.GetArmArmorSum();
                    break;
            }
            
            float[] resistancePercentages = new float[5];
            
            
            
            
            // get attacker information
           
            
            
                if (!Victim.IsHero)
                {
                    //unit attributes
                    var defenseProperties = Victim.Character.GetDefenseProperties();
                
                    foreach (var property in defenseProperties)
                    {
                        damagePercentages[(int) property.ResistedDamageType] += property.ReductionPercent;
                    }
                
                    //add temporary effects like buffs to defenese bonuses
                    List<ItemTrait> itemTraits = Victim.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(Victim.WieldedWeapon.Item);
                
                    foreach (var temporaryTraits in itemTraits)
                    {
                        var defenseProperty = temporaryTraits.DefenseProperty;
                        if (defenseProperty != null)
                        {
                            damagePercentages[(int) temporaryTraits.DefenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                        }
                    }
                
                }
                else
                {
                    //Hero item level attributes 

                    List<ItemTrait> itemTraits= new List<ItemTrait>();
                    List<ItemObject> items;

                    // get all equipment Pieces

                    items = Victim.Character.GetCharacterEquipment();
                    foreach (var item in items)
                    {
                        if(item.HasTrait())
                            itemTraits.AddRange(item.GetTraits());
                    
                        itemTraits.AddRange(Victim.GetComponent<ItemTraitAgentComponent>().GetDynamicTraits(item));
                    }

                    foreach (var itemTrait in itemTraits)
                    {
                        var defenseProperty = itemTrait.DefenseProperty;
                        if(defenseProperty==null) 
                            continue;
                    
                        damagePercentages[(int)defenseProperty.ResistedDamageType] += itemTrait.DefenseProperty.ReductionPercent;
                    }
                
                
                    //add temporary effects like buffs to defenese bonuses
                    List<ItemTrait> dynamicTraits = attacker.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(attacker.WieldedWeapon.Item);
                
                    foreach (var temporaryTraits in dynamicTraits)
                    {
                        var defenseProperty = temporaryTraits.DefenseProperty;
                        if (defenseProperty != null)
                        {
                            damagePercentages[(int) temporaryTraits.DefenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                        }
                    }
                }
            
            //reading out Status Effect Component of Victim we probably should redesign status effects 
            StatusEffectComponent.EffectAggregate effectAggregate = Victim.GetComponent<StatusEffectComponent>().GetEffectAggregate();
            damageReduction += effectAggregate.PercentageArmorEffect;
            armorPenetration += effectAggregate.PercentageArmorEffect;
            
            // final calculation
            float physical = b.InflictedDamage;
            //armor penetration is only calculated if physical damage is 
            //extra damage is added on top by armor penetration, which is applied only to the maximum value of the armor

            physical = physical + Math.Min(armorPenetration, armor);
            
            // damage is converted partly to the target non-physical damage type, rest resides physical
            var nonPhysical = 0f;
            if (additionalDamageType != DamageType.Physical)
            {
                nonPhysical = physical;
                physical = Convertion * physical;
                nonPhysical -= physical;
            }
           
            
            //modifiers were calculated, by subtracting damage type percentage and resistance type percentage
            damagePercentages[0]= (damagePercentages[0] - resistancePercentages[0]);
            damagePercentages[(int) additionalDamageType]= 1+ (damagePercentages[(int) additionalDamageType] - resistancePercentages[(int) additionalDamageType]);
            //modify damage

            physical *=1+  damagePercentages[0];
            nonPhysical *=1+  damagePercentages[(int)additionalDamageType];
            
            //calculate general amplification and WardSave 
            damageAmplification = 1 + damageAmplification - damageReduction;

          

            // calculation for resistances and damag
            b.InflictedDamage = (int)(damageAmplification * (physical + nonPhysical));
            if (nonPhysical > 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("damaged by"+ b.InflictedDamage+ "with transformed " + nonPhysical, new Color(0, 250, 250)));
            }
            
            
            return true;

        }
    }
}