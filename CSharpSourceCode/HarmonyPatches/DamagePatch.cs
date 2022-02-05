using System.Globalization;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Damage;
using TOW_Core.ObjectDataExtensions;
using TOW_Core.Utilities.Extensions;
namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class DamagePatch
    {
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
            float[] damageCategories=new float[(int) DamageType.All+1];
            var attackerPropertyContainer = attacker.GetProperties(PropertyMask.Attack);
            var victimPropertyContainer = victim.GetProperties(PropertyMask.Defense);
            //attack properties;
            var damageProportions = attackerPropertyContainer.DamageProportions;
            var damagePercentages = attackerPropertyContainer.DamagePercentages;
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

            b.InflictedDamage = resultDamage;

            if (b.InflictedDamage > 0)
            {
                if(attacker==Agent.Main || victim==Agent.Main)
                    DisplayDamageResult(resultDamage, damageCategories);
            }
            return true;
        }

        private static void DisplaySpellDamageResult(string SpellName, DamageType additionalDamageType, 
            int resultDamage, float damageAmplifier)
        {
            var displayColor = Color.White;
            string displayDamageType = "";

            switch (additionalDamageType)
            {
                case DamageType.Fire:
                    displayColor = Colors.Red;
                    displayDamageType = "fire";
                    break;
                case DamageType.Holy:
                    displayColor = Colors.Yellow;
                    displayDamageType = "holy";
                    break;
                case DamageType.Lightning:
                    displayColor = Colors.Blue;
                    displayDamageType = "lightning";
                    break;
                case DamageType.Magical:
                    displayColor = Colors.Cyan;
                    displayDamageType = "magical";
                    break;
                case DamageType.Physical:
                    displayColor = Color.White;
                    displayDamageType = "Physical";
                    break;
            }
            InformationManager.DisplayMessage(new InformationMessage(resultDamage + "cast damage consisting of  "+" ("+displayDamageType +") was applied "+ "which was modified by " + (1+damageAmplifier).ToString("##%", CultureInfo.InvariantCulture) , displayColor));
        }
        

        private static void DisplayDamageResult(int resultDamage, float[] categories)
        {
            var displaycolor = Color.White;
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
                    string s = ", " +(int) categories[i] + " was dealt in " + t;
                    if (additionalDamageTypeText == "")
                        additionalDamageTypeText = s;
                    else
                        additionalDamageTypeText.Add(s,false);
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

            var resultText = (int) resultDamage+ " damage was dealt of which was "+ (int) categories[1]+ " "+ nameof(DamageType.Physical)+additionalDamageTypeText;
            InformationManager.DisplayMessage(new InformationMessage(resultText, displaycolor));
            
        }
    }
}