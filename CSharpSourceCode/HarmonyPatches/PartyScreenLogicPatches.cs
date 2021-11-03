using HarmonyLib;
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
            raiseDeadBehavior.LastNumberOfTroopsRaised = 0;
        }
    }
}
