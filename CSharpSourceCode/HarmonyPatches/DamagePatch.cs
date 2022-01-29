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
using TOW_Core.Abilities;
using TOW_Core.Battle.Damage;
using TOW_Core.Battle.StatusEffects;
using TOW_Core.Battle.TriggeredEffect;
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
            
            if (victim.IsMount || attacker.IsMount)
            {
                return true;
            }

            bool isSpell = false;

            var additionalDamageType = DamageType.Physical;
            var transformation = 0.5f;

            var attackerPropertyContainer = attacker.GetProperties(PropertyFlag.Attack);
            var victimPropertyContainer = victim.GetProperties(PropertyFlag.Defense);

            //attack properties;
            var damagePercentages = attackerPropertyContainer.DamagePercentages;
            var damageAmplification = attackerPropertyContainer.Amplifier;
            additionalDamageType = attackerPropertyContainer.DamageType;
            
            //defense properties
            
            var resistancePercentages = victimPropertyContainer.ResistancePercentages;
            var damageReduction = victimPropertyContainer.WardSave;



            if (b.StrikeType == StrikeType.Invalid && b.AttackType == AgentAttackType.Kick && b.DamageCalculated)
            {
                //apply here spell properties
                isSpell = true;
                additionalDamageType = DamageType.Magical;
                transformation = 1;
            }
            
            float physical=b.InflictedDamage;
            var nonPhysical = 0f;

            if (isSpell)
            {
                var spellInfo = SpellBlowInfoManager.GetSpellInfo(attacker.Index);

                 nonPhysical =(int) physical;
                additionalDamageType = DamageType.Magical;
                transformation = 1;
                damageAmplification = 1 + (damageAmplification - damageReduction);
                b.InflictedDamage = (int)(damageAmplification * nonPhysical);
                DisplaySpellDamageResult(spellInfo.SpellID,additionalDamageType,b.InflictedDamage,damagePercentages, nonPhysical);
                return true;
            }
            
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
            damageAmplification= 1 + damageAmplification - damageReduction;
                

            var resultDamage = (int)(damageAmplification * (physical + nonPhysical));

            DisplayDamageResult(additionalDamageType, nonPhysical, resultDamage, damagePercentages, physical);

            b.InflictedDamage = resultDamage;

            return true;
    

            //defender information

                //armor for penetration otherwise no use since the physical calculation has already taken place.
        }

        private static void DisplaySpellDamageResult(string SpellName, DamageType additionalDamageType, 
            int resultDamage, float[] damagePercentages, float nonPhysical)
        {
            var displaycolor = new Color();
            string displayDamageType = "";

            switch (additionalDamageType)
            {
                case DamageType.Fire:
                    displaycolor = Colors.Red;
                    displayDamageType = "fire";
                    break;
                case DamageType.Holy:
                    displaycolor = Colors.Yellow;
                    displayDamageType = "holy";
                    break;
                case DamageType.Lightning:
                    displaycolor = Colors.Blue;
                    displayDamageType = "lightning";
                    break;
                case DamageType.Magical:
                    displaycolor = Colors.Cyan;
                    displayDamageType = "magical";
                    break;
                case DamageType.Physical:
                    displaycolor = Color.White;
                    displayDamageType = "Physical";
                    break;
            }
            
            InformationManager.DisplayMessage(new InformationMessage(resultDamage + "cast damage applied by "+SpellName+" ("+displayDamageType +") was applied "+ "which was modified by " + (1+damagePercentages[(int) additionalDamageType]).ToString("##%", CultureInfo.InvariantCulture) , displaycolor));
        }
        

        private static void DisplayDamageResult(DamageType additionalDamageType, float nonPhysical,
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
            

            if (nonPhysical > 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "damaged by " + resultDamage + "with " + nonPhysical + " transformed to " + displaytext + " damage " +
                    " modified by " +
                    (1 + damagePercentages[(int)additionalDamageType]).ToString("##%", CultureInfo.InvariantCulture),
                    displaycolor));
                InformationManager.DisplayMessage(new InformationMessage(
                    "physical part " + (physical) + " with modifier " +
                    (1 + damagePercentages[0]).ToString("##%", CultureInfo.InvariantCulture), Colors.White));
            }
            else
            {
                if(physical>0&& damagePercentages[0]>0)
                    InformationManager.DisplayMessage(new InformationMessage(
                    "physical part " + (physical) + " with modifier " +
                    (1 + damagePercentages[0]).ToString("##%", CultureInfo.InvariantCulture), Colors.White));
            }
        }
    }
}