using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.InputSystem;

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
    }
}
