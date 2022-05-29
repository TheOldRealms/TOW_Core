using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class AgentPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Agent), "GetBattleImportance")]
        public static void BattleImportancePatch(ref float __result, Agent __instance)
        {
            if (__instance.IsExpendable())
            {
                __result = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Formation), "GetMedianAgent")]
        public static void MedianAgentPatch(ref Agent __result, Formation __instance)
        {
            if(__result == null)
            {
                List<Agent> units = new List<Agent>();
                foreach(var unit in __instance.Arrangement.GetAllUnits())
                {
                    units.Add((Agent)unit);
                }
                __result = units.FirstOrDefault();
            }
        }
    }
}
