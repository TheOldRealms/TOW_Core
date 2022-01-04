using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.Core;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public class ItemObjectPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemObject), "GetAirFrictionConstant")]
        public static void PostFix(ref float __result, WeaponClass weaponClass)
        {
            if(weaponClass == WeaponClass.Boulder)
            {
                __result = 0.0001f;
            }
        }
    }
}
