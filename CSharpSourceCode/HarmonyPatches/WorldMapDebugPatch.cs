using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

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
                //teleport party to a valid navmesh position.
                __instance.Position2D = Settlement.All.First().GatePosition;
                /*
                var debug = SettlementHelper.FindNearestVillage(null, __instance);
                if (debug == null)
                {
                    
                    
                }*/
            }
            return true;
        }
    }
}
