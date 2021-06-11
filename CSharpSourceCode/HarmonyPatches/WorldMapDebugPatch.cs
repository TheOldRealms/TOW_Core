using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.HarmonyPatches 
{ 
    [HarmonyPatch]
    public static class WorldMapDebugPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MobileParty), "RecoverPositionsForNavMeshUpdate")]
        public static bool PreFix(ref MobileParty __instance)
        {
            if (__instance.Position2D.IsNonZero() && !PartyBase.IsPositionOkForTraveling(__instance.Position2D))
            {
                var debug = SettlementHelper.FindNearestSettlementToMapPoint(__instance, (Settlement s) => s.IsVillage);
                if (debug == null)
                {
                    //teleport party to a valid navmesh position.
                    __instance.Position2D = Settlement.All.First().GatePosition;
                }
            }
            return true;
        }
    }
}
