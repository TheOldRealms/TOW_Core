using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    public class CharacterObjectPatches
    {
        [HarmonyPatch]
        class Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterObject), "TroopWage", MethodType.Getter)]
            public static bool TroopWagePrefix(ref int __result, CharacterObject __instance)
            {
                if (__instance.IsUndead())
                {
                    __result = 0;
                    return false;
                }
                return true;
            }
        }
    }
}
