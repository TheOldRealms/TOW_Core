using System;
using System.Collections.Generic;
using System.Globalization;
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
            Agent victim = __instance;

            bool isSpell = false;


            if (victim.IsMount || attacker.IsMount)
            {
                return true;
            }
            
                //currently unused need assignment in data table
            var transformation=0.5f;        //called it previously "conversion"
            var damageAmplification=0.0f;
            var damageReduction = 0f;
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
                 //the ultimate comparision if the blow is a spell 
                 if (b.StrikeType == StrikeType.Invalid && b.AttackType == AgentAttackType.Kick && b.DamageCalculated)
                 {
                     //apply here spell properties
                     isSpell = true;
                     additionalDamageType = DamageType.Magical;
                     transformation = 1;
                 }
                 else
                 {
                     var weaponProperty = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                     if (weaponProperty != null)
                     {
                         damagePercentages[(int)weaponProperty.DamageType] += 0; //should provide some additional damage maybe?
                         additionalDamageType = weaponProperty.DamageType;
                         // chose damage type depending on dominant override type 
                         if (weaponProperty.DamageType == DamageType.Physical)
                         {
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
                         }
                     }
                 }
                 
                 
             }

            //defender information

                //armor for penetration otherwise no use since the physical calculation has already taken place.
            var victimEquipment = victim.Character.Equipment;
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

                if (!victim.IsHero)
                {
                    //unit attributes
                    var defenseProperties = victim.Character.GetDefenseProperties();
                
                    foreach (var property in defenseProperties)
                    {
                        resistancePercentages[(int) property.ResistedDamageType] += property.ReductionPercent;
                    }
                
                    //add temporary effects like buffs to defenese bonuses
                    List<ItemTrait> itemTraits = victim.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(victim.WieldedWeapon.Item);
                
                    foreach (var temporaryTraits in itemTraits)
                    {
                        var defenseProperty = temporaryTraits.DefenseProperty;
                        if (defenseProperty != null)
                        {
                            resistancePercentages[(int) temporaryTraits.DefenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                        }
                    }
                }
                else
                {
                    //Hero item level attributes 

                    List<ItemTrait> itemTraits= new List<ItemTrait>();
                    List<ItemObject> items;

                    // get all equipment Pieces

                    items = victim.Character.GetCharacterEquipment();
                    foreach (var item in items)
                    {
                        if(item.HasTrait())
                            itemTraits.AddRange(item.GetTraits());
                    
                        itemTraits.AddRange(victim.GetComponent<ItemTraitAgentComponent>().GetDynamicTraits(item));
                    }

                    foreach (var itemTrait in itemTraits)
                    {
                        var defenseProperty = itemTrait.DefenseProperty;
                        if(defenseProperty==null) 
                            continue;
                    
                        resistancePercentages[(int)defenseProperty.ResistedDamageType] += itemTrait.DefenseProperty.ReductionPercent;
                    }
                
                
                    //add temporary effects like buffs to defenese bonuses
                    List<ItemTrait> dynamicTraits = attacker.GetComponent<ItemTraitAgentComponent>()
                        .GetDynamicTraits(attacker.WieldedWeapon.Item);
                
                    foreach (var temporaryTraits in dynamicTraits)
                    {
                        var defenseProperty = temporaryTraits.DefenseProperty;
                        if (defenseProperty != null)
                        {
                            resistancePercentages[(int) temporaryTraits.DefenseProperty.ResistedDamageType] += defenseProperty.ReductionPercent;
                        }
                    }
                }
            
            //reading out Status Effect Component of Victim we probably should redesign status effects 
            StatusEffectComponent.EffectAggregate effectAggregate = victim.GetComponent<StatusEffectComponent>().GetEffectAggregate();
            damageReduction += effectAggregate.PercentageArmorEffect;
            armorPenetration += effectAggregate.PercentageArmorEffect;
            
            // final calculation
            float physical = b.InflictedDamage;
            
            var nonPhysical = 0f;
            
            if (isSpell)
            {
                nonPhysical = physical;
                additionalDamageType = DamageType.Magical;
                damagePercentages[(int) additionalDamageType] -= resistancePercentages[(int) additionalDamageType];
                nonPhysical *=1+  damagePercentages[(int)additionalDamageType];
                damageAmplification = 1 + damageAmplification - damageReduction;
                
                b.InflictedDamage = (int)(damageAmplification * nonPhysical);
                
                DisplaySpellDamageResult(additionalDamageType,b.InflictedDamage,damagePercentages,nonPhysical);
                return true;
            }

            //armor penetration is only calculated if physical damage is 
            //extra damage is added on top by armor penetration, which is applied only to the maximum value of the armor
            physical = physical + Math.Min(armorPenetration, armor);
            
            // damage is converted partly to the target non-physical damage type, rest resides physical
            
            if (additionalDamageType != DamageType.Physical)
            {
                nonPhysical = physical;
                physical = transformation * physical;
                nonPhysical -= physical;
            }
           
            
            //modifiers were calculated, by subtracting damage type percentage and resistance type percentage
            damagePercentages[0]-= resistancePercentages[0];
            damagePercentages[(int) additionalDamageType] -= resistancePercentages[(int) additionalDamageType];
            
            
            //modify damage
            physical *=1+  damagePercentages[0];
            nonPhysical *=1+  damagePercentages[(int)additionalDamageType];
            
            //calculate general amplification and WardSave 
            damageAmplification = 1 + damageAmplification - damageReduction;
            
            

            
            
            var resultDamage = (int)(damageAmplification * (physical + nonPhysical));
            

            DisplayDamageResult(additionalDamageType, armorPenetration, nonPhysical, resultDamage, damagePercentages, physical);

            b.InflictedDamage = resultDamage;
            
            return true;

        }

        private static void DisplaySpellDamageResult(DamageType additionalDamageType, 
            int resultDamage, float[] damagePercentages, float nonPhysical)
        {
            var displaycolor = new Color();
            string displaytext = "";

            switch (additionalDamageType)
            {
                case DamageType.Fire:
                    displaycolor = Colors.Red;
                    displaytext = "fire";
                    break;
                case DamageType.Holy:
                    displaycolor = Colors.Yellow;
                    displaytext = "holy";
                    break;
                case DamageType.Lightning:
                    displaycolor = Colors.Blue;
                    displaytext = "lightning";
                    break;
                case DamageType.Magical:
                    displaycolor = Colors.Cyan;
                    displaytext = "magical";
                    break;
                case DamageType.Physical:
                    displaycolor = Color.White;
                    displaytext = "Physical";
                    break;
            }
            
            InformationManager.DisplayMessage(new InformationMessage(resultDamage + "cast damage applied as "+displaytext +" damage" , displaycolor));
        }
        

        private static void DisplayDamageResult(DamageType additionalDamageType, float armorPenetration, float nonPhysical,
            int resultDamage, float[] damagePercentages, float physical)
        {
            var displaycolor = new Color();
            string displaytext = "";

            switch (additionalDamageType)
            {
                case DamageType.Fire:
                    displaycolor = Colors.Red;
                    displaytext = "fire";
                    break;
                case DamageType.Holy:
                    displaycolor = Colors.Yellow;
                    displaytext = "holy";
                    break;
                case DamageType.Lightning:
                    displaycolor = Colors.Blue;
                    displaytext = "Lightning";
                    break;
                case DamageType.Magical:
                    displaycolor = Colors.Cyan;
                    displaytext = "Magical";
                    break;
                case DamageType.Physical:
                    displaycolor = Color.White;
                    displaytext = "Physical";
                    break;
            }

           

            if (armorPenetration > 0)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage("additional armor penetration damage " + armorPenetration,
                        new Color(100, 100, 100)));
            }

            if (nonPhysical > 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "damaged by " + resultDamage + "with " + nonPhysical + " transformed to " + displaytext + " damage " +
                    " modified by " +
                    (1 + damagePercentages[(int)additionalDamageType]).ToString("P", CultureInfo.InvariantCulture),
                    displaycolor));
                InformationManager.DisplayMessage(new InformationMessage(
                    "physical part " + (physical + armorPenetration) + " with modifier " +
                    (1 + damagePercentages[0]).ToString("P", CultureInfo.InvariantCulture), Colors.White));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "physical part " + (physical + armorPenetration) + " with modifier " +
                    (1 + damagePercentages[0]).ToString("P", CultureInfo.InvariantCulture), Colors.White));
            }
        }
    }
}