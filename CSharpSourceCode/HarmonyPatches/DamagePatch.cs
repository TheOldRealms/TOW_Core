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
                ItemDamageProperty damageProperty;

                float ArmorPenetration=0f;
                float bonusDamage=0f;

                DamageType overrideDamageType= DamageType.Physical;



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

                    var tmpDamageType = itemTrait.OffenseProperty.DefaultDamageTypeOverride;
                    if (tmpDamageType != DamageType.Physical)
                    {
                        if (tmpDamageType == DamageType.Fire &&overrideDamageType != DamageType.Magical)
                        {
                            continue;
                        }
                        overrideDamageType = tmpDamageType;
                    }
                }

                var resultDamageType = DamageType.Physical;
                
                //weapon properties
                var damageProperties = attacker.WieldedWeapon.Item.GetTorSpecificData().ItemDamageProperty;
                if (damageProperties != null)
                {
                    
                    
                    if (overrideDamageType != DamageType.Physical)
                    {
                        if (damageProperties.DamageType == DamageType.Fire &&overrideDamageType != DamageType.Magical)
                        {
                        }
                        else
                        {
                            resultDamageType = damageProperties.DamageType;
                        }
                    }
                    else
                    {
                        resultDamageType = damageProperties.DamageType;
                    }
                    
                    
                    
                    // maximum minimum is really not needed. just go with objects.xml in the prenative module do it here anywhere
                    Mathf.Clamp(b.InflictedDamage, damageProperties.MinDamage, damageProperties.MaxDamage);
                }
                
                
                
                
                
                
            }
            



            return true;

        }
    }
}