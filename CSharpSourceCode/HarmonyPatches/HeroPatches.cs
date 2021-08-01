using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(__instance.HasAttribute("VampireBodyOverride"))
            {
                try
                {
                    StaticBodyProperties staticBodyProperties = (StaticBodyProperties)Traverse.Create(__instance).Property("StaticBodyProperties").GetValue();
                    __result = new BodyProperties(new DynamicBodyProperties(19, __instance.Weight, __instance.Build), staticBodyProperties);
                }
                catch(Exception e)
                {
                    TOW_Core.Utilities.TOWCommon.Log("Attempted to override BodyProperties for Hero, but failed.", NLog.LogLevel.Error);
                }
            }
        }
    }
}
