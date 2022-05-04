using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SandBox;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class AtmoshpereOverridePatch
    {
        private static readonly string _forceAtmosphereKey = "forceatmo";

        //Ideally we should replace the entire weathermodel and not use a harmonypatch for this. 
        //But a lot of the methods are private and not protected, 
        //so access is troublesome if we would implement a new model derived off of the default one.
        //A lot of functionality to rewrite.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DefaultMapWeatherModel), "GetNormalizedSnowValueInPos")]
        public static void TurnOffSnow(ref float __result)
        {
            __result = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SandBoxMissions), "CreateSandBoxMissionInitializerRecord")]
        public static void ForceSceneAtmosphere(string sceneName, MissionInitializerRecord __result)
        {
            if (sceneName.Contains(_forceAtmosphereKey))
            {
                //__result.AtmosphereOnCampaign = new AtmosphereInfo { AtmosphereName = "" };
            }
        }
    }
}
