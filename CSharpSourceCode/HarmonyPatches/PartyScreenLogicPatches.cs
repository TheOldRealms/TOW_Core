using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TOW_Core.CampaignSupport.RaiseDead;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class PartyScreenLogicPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PartyScreenLogic), "LeftPartySizeLimit", MethodType.Getter)]
        public static void AddRaiseDeadCountToTroopLimit(ref int __result)
        {
            RaiseDeadCampaignBehavior raiseDeadBehavior = Campaign.Current.GetCampaignBehavior<RaiseDeadCampaignBehavior>();
            __result += raiseDeadBehavior.LastNumberOfTroopsRaised;
        }
    }
}
