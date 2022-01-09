using System.Collections.Generic;
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
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Agent), "HandleBlow")]
        public static bool PreHandleBlow(ref Blow b, ref Agent __instance)
        {
            //check for new Damage system, will change that 

            Agent attacker = b.OwnerId != -1 ? Mission.Current.FindAgentWithIndex(b.OwnerId) : __instance;
            Agent Victim = __instance;
            
            // get attacker information
            if (!attacker.IsHero)
            {
                //soldier attributes
            }
            else
            {
                //item level attributes

                List<ItemTrait> itemTraits= new List<ItemTrait>();
                List<ItemObject> items;

                float ArmorPenetration=0f;
                float bonusDamage=0f;

                DamageType damageType= DamageType.Physical;



                    // get all equipment Pieces

                items = attacker.Character.GetCharacterEquipment();
                foreach (var item in items)
                {
                    itemTraits.AddRange(item.GetTraits());


                    itemTraits.AddRange(attacker.GetComponent<ItemTraitAgentComponent>().GetDynamicTraits(item));
                }

                foreach (var itemTrait in itemTraits)
                {
                    ArmorPenetration += itemTrait.OffenseProperty.ArmorPenetration;
                    bonusDamage += itemTrait.OffenseProperty.BonusDamagePercent;

                    var tmpDamageType = itemTrait.OffenseProperty.DefaultDamageTypeOverride;
                    if (tmpDamageType != DamageType.Physical)
                    {
                        if (tmpDamageType == DamageType.Fire &&damageType != DamageType.Magical)
                        {
                            damageType = tmpDamageType;
                            continue;
                        }
                        damageType = tmpDamageType;
                    }
                }
            }
            



            return true;

        }
    }
}