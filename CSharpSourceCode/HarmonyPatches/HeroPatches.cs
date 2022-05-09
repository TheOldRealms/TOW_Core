using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class HeroPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "BodyProperties", MethodType.Getter)]
        public static void PostFix(ref BodyProperties __result, Hero __instance)
        {
            if (__instance.IsVampire())
            {
                StaticBodyProperties staticBodyProperties = (StaticBodyProperties)Traverse.Create(__instance).Property("StaticBodyProperties").GetValue();
                __result = new BodyProperties(new DynamicBodyProperties(19, __instance.Weight, __instance.Build), staticBodyProperties);
            }
            else if (__instance.Age < 26)
            {
                StaticBodyProperties staticBodyProperties = (StaticBodyProperties)Traverse.Create(__instance).Property("StaticBodyProperties").GetValue();
                __result = new BodyProperties(new DynamicBodyProperties(26, __instance.Weight, __instance.Build), staticBodyProperties);
            }
        }
    }
}
