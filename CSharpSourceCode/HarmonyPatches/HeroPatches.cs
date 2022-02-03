using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class HeroPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "BodyProperties", MethodType.Getter)]
        public static void PostFix(ref BodyProperties __result, Hero __instance)
        {
            if (__instance.IsNotable)
            {
                if (__instance.Culture.Name.Contains("Vampire"))
                {
                    StaticBodyProperties staticBodyProperties = (StaticBodyProperties)Traverse.Create(__instance).Property("StaticBodyProperties").GetValue();
                    __result = new BodyProperties(new DynamicBodyProperties(19, __instance.Weight, __instance.Build), staticBodyProperties);
                }
                else if (!__instance.IsFemale)
                {
                    StaticBodyProperties staticBodyProperties = (StaticBodyProperties)Traverse.Create(__instance).Property("StaticBodyProperties").GetValue();
                    __result = new BodyProperties(new DynamicBodyProperties(21, __instance.Weight, __instance.Build), staticBodyProperties);
                }
            }
        }
    }
}
