using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOW_Core.Texts;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch(typeof(GameTextManager))]
    public static class FindGameTextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("FindText")]
        public static void PostFix(ref TextObject __result, GameTextManager __instance, string id, string variation = null)
        {
            TextObject text = null;
            if(TOWTextManager.TryGetOverrideFor(id, out text, variation))
            {
                __result = text;
            }
        }
    }
}
