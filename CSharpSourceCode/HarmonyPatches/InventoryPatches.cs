using HarmonyLib;
using SandBox.GauntletUI;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOW_Core.Battle.Damage;
using TOW_Core.Items;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class InventoryPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGauntletScreen), "OnFrameTick")]
        public static void Postfix(SPInventoryVM ____dataSource)
        {
            if (InputKey.Tilde.IsPressed())
            {
                Utilities.TOWCommon.CopyEquipmentToClipBoard(____dataSource);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemMenuVM), "SetItem")]
        public static void Postfix2(ref ItemMenuVM __instance, SPItemVM item)
        {
            var props = ExtendedItemObjectManager.GetAdditionalProperties(item.StringId);
            if(props != null)
            {
                /*
                if(props.DamageProportions != null && props.DamageProportions.Count > 0)
                {
                    foreach(var p in props.DamageProportions)
                    {
                        __instance.TargetItemProperties.Add(new ItemMenuTooltipPropertyVM(p.DamageType.ToString(), "test", 0, GetColorForDamageType(p.DamageType), false));
                    }
                }
                */
            }
        }

        private static Color GetColorForDamageType(DamageType type)
        {
            switch (type)
            {
                case DamageType.Physical:
                    return new Color { Red=63, Green=63, Blue=63, Alpha=1};
                case DamageType.Magical:
                    return new Color { Red = 255, Green = 165, Blue = 90, Alpha = 1 };
                case DamageType.Fire:
                    return new Color { Red = 1, Green = 165, Blue = 255, Alpha = 1 };
                default:
                    return Color.White;

            }
        }
    }
}
