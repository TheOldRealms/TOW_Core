using System.Runtime.CompilerServices;
using HarmonyLib;
using NLog;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TOW_Core.Utilities;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CrossHairPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MissionGauntletCrosshair), "GetShouldCrosshairBeVisible")]
        public static void PostFix(ref bool  __result)
        {
            if (Mission.Current.MainAgent !=null && Mission.Current.MainAgent.WieldedWeapon.Ammo>0)
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
        }
        
    }
}