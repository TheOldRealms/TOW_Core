using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TOW_Core.Battle.Map;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class LocationPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Location), "GetSceneName")]
        public static void SetSceneNameInWeatherModel(ref string __result)
        {
            AtmosphereOverrideMissionLogic.currentSceneName = __result;
        }
    }
}
