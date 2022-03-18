using System.Globalization;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Damage;
using TOW_Core.Battle.Sound;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Agent), "HandleBlow")]
        public static bool PreHandleBlow(ref Blow b, Agent __instance)
        {
            Agent attacker = b.OwnerId != -1 ? Mission.Current.FindAgentWithIndex(b.OwnerId) : __instance;
            Agent victim = __instance;
            
            if (!victim.IsHuman || !attacker.IsHuman)
            {
                return true;
            }
            
            if (attacker == victim)
            {
                return true;
            }
            
            
            SoundEvent _hitsound;

            bool isSpell = false;
            float[] damageCategories=new float[(int) DamageType.All+1];
            var attackerPropertyContainer = attacker.GetProperties(PropertyMask.Attack);
            var victimPropertyContainer = victim.GetProperties(PropertyMask.Defense);
            //attack properties;
            var damageProportions = attackerPropertyContainer.DamageProportions;
            var damagePercentages = attackerPropertyContainer.DamagePercentages;
            var additionalDamagePercentages = attackerPropertyContainer.AdditionalDamagePercentages;
            //defense properties
            var resistancePercentages = victimPropertyContainer.ResistancePercentages;

           
            
            if (b.StrikeType == StrikeType.Invalid && b.AttackType == AgentAttackType.Kick && b.DamageCalculated)
            {
                isSpell = true;
            }
            
            if (isSpell)
            {
                var spellInfo = SpellBlowInfoManager.GetSpellInfo(victim.Index,attacker.Index);
                int damageType = (int) spellInfo.DamageType;
                damageCategories[damageType] = b.InflictedDamage;
                damagePercentages[damageType] -= resistancePercentages[damageType];
                damageCategories[damageType] *= 1 + damagePercentages[damageType];
                b.InflictedDamage = (int)damageCategories[damageType];
                if(attacker==Agent.Main || victim==Agent.Main)
                    TORDamageDisplay.DisplaySpellDamageResult(spellInfo.SpellID,spellInfo.DamageType,b.InflictedDamage,damagePercentages[damageType]);
                return true;
            }

            var resultDamage = 0;
            var highestDamageValue =0f;
            var highestNonPhysicalDamageType = DamageType.Physical;
            for (int i = 0; i < damageCategories.Length-1; i++)
            {
                damageProportions[i] += additionalDamagePercentages[i];
                damageCategories[i] = b.InflictedDamage * damageProportions[i];
                damageCategories[i] += damageCategories[(int)DamageType.All]/(int) DamageType.All;
                if (damageCategories[i] > 0)
                {
                    if (damageCategories[i] > highestDamageValue&& i!= (int) DamageType.Physical)
                    {
                        highestDamageValue = damageCategories[i];
                        highestNonPhysicalDamageType = (DamageType)i;
                    }
                    damagePercentages[i] -= resistancePercentages[i];
                    damageCategories[i] *= 1 + damagePercentages[i];
                    resultDamage += (int) damageCategories[i];
                }
            }

            if (highestNonPhysicalDamageType != DamageType.Physical)
            {
                victim.GetComponent<AgentSoundComponent>().PlayHitSound(highestNonPhysicalDamageType);
            }
            

            b.InflictedDamage = resultDamage;

            if (b.InflictedDamage > 0)
            {
                if(attacker==Agent.Main || victim==Agent.Main)
                    TORDamageDisplay.DisplayDamageResult(resultDamage, damageCategories);
            }
            
            return true;
        }


        

        
    }
}