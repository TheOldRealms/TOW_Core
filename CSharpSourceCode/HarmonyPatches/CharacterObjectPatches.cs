using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TOW_Core.CampaignSupport.Models;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CharacterObjectPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "TroopWage", MethodType.Getter)]
        public static bool TroopWagePrefix(ref int __result, CharacterObject __instance)
        {
            if (__instance.IsUndead())
            {
                __result = 0;
            }
            else
            {
                var model = Campaign.Current.Models.PartyWageModel;
                __result = model.GetCharacterWage(__instance.Tier);
                TOWCommon.Say($"{model.GetType().Name}");
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterObject), "Tier", MethodType.Getter)]
        public static bool TierPrefix(ref int __result, CharacterObject __instance)
        {
            if (__instance.IsHero)
            {
                __result = 0;
            }
            else
            {
                __result = Math.Min(Math.Max(MathF.Ceiling(((float)__instance.Level - 5f) / 5f), 0), 9);
            }
            return false;
        }
    }
}
