using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            
            float[] damageCategories=new float[7];

            var attackerPropertyContainer = attacker.GetProperties(PropertyFlag.Attack);
            var victimPropertyContainer = victim.GetProperties(PropertyFlag.Defense);

            //attack properties;
            var damageProportions = attackerPropertyContainer.DamageProportions;
            var damagePercentages = attackerPropertyContainer.DamagePercentages;
            
            //defense properties
            var resistancePercentages = victimPropertyContainer.ResistancePercentages;

            if (b.StrikeType == StrikeType.Invalid && b.AttackType == AgentAttackType.Kick && b.DamageCalculated)
            {
                //apply here spell properties
                isSpell = true;
            }
            
            if (isSpell)
            {
                //for now 100% converted to target type
                var spellInfo = SpellBlowInfoManager.GetSpellInfo(attacker.Index);
                int damageType = (int) spellInfo.DamageType;
                damageCategories[damageType] = b.InflictedDamage;
                damagePercentages[damageType] -= resistancePercentages[damageType];
                damageCategories[damageType] *= 1 + damagePercentages[damageType];
                b.InflictedDamage = (int)damageCategories[damageType];
                DisplaySpellDamageResult(spellInfo.SpellID,spellInfo.DamageType,b.InflictedDamage,damagePercentages[damageType]);
                return true;
            }

            var resultDamage = 0;
            for (int i = 0; i < damageCategories.Length; i++)
            {
                damageCategories[i] = b.InflictedDamage * damageProportions[i];

                if (damageCategories[i] > 0)
                {
                    damagePercentages[i] -= resistancePercentages[i];
                    damageCategories[i] *=1+ damagePercentages[i];
                    resultDamage +=(int) damageCategories[i];
                }
            }


            DisplayDamageResult(resultDamage, damageCategories, damagePercentages);

            b.InflictedDamage = resultDamage;

            return true;
            
        }

        private static void DisplaySpellDamageResult(string SpellName, DamageType additionalDamageType, 
            int resultDamage, float damageAmplifier)
        {
            var displaycolor = Color.White;
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
            
            InformationManager.DisplayMessage(new InformationMessage(resultDamage + "cast damage applied by "+SpellName+" ("+displayDamageType +") was applied "+ "which was modified by " + (1+damageAmplifier).ToString("##%", CultureInfo.InvariantCulture) , displaycolor));
        }
        

        private static void DisplayDamageResult(int resultDamage, float[] categories, float[] amplifier)
        {
            var displaycolor = Color.White;
            string displaytext = "";

            var dominantAdditionalEffect = DamageType.Physical;
            float dominantCategory=0;
            string additionalDamageTypeText= "";
            for (int i = 2; i < categories.Length; i++) //starting from first real additional damage type
            {
                if (dominantCategory < categories[i])
                {
                    dominantCategory = categories[i];
                    dominantAdditionalEffect = (DamageType) i;
                }

                if (categories[i] > 0)
                {
                    DamageType t = (DamageType)i;
                    string s = ", " + categories[i] + " was dealt in " + t;
                    if (additionalDamageTypeText == "")
                    {
                        additionalDamageTypeText = s;
                    }
                    else
                    {
                        additionalDamageTypeText.Add(s,false);
                    }
                }
            }

            switch (dominantAdditionalEffect)
            {
                case DamageType.Fire:
                    displaycolor = Colors.Red;
                    break;
                case DamageType.Holy:
                    displaycolor = Colors.Yellow;
                    break;
                case DamageType.Lightning:
                    displaycolor = Colors.Blue;
                    break;
                case DamageType.Magical:
                    displaycolor = Colors.Cyan;
                    break;
            }

            string resultText;
            resultText = resultDamage+ " was dealt of which was "+ categories[1]+ " "+ nameof(DamageType.Physical)+additionalDamageTypeText ;
            InformationManager.DisplayMessage(new InformationMessage(resultText, displaycolor));
            
        }
    }
}