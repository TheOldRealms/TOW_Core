using HarmonyLib;
using TaleWorlds.Core;
using TOW_Core.Utilities;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class ArtilleryPatches
    {
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemObject),"GetAirFrictionConstant")]
        public static void PostFix(ref float __result, ItemObject __instance, WeaponFlags weaponFlags)
        {
            
        }
        
    }
}