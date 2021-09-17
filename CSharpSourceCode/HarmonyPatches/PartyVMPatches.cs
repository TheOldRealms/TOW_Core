using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TOW_Core.CampaignSupport.RaiseDead;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class PartyVMPatches
    {
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PartyVM), "InitializePartyList")]
        public static void AddRaiseDeadToOtherTroops(ref TroopRoster currentTroopRoster, ref PartyScreenLogic.TroopType type, int side)
        {
            if (type.Equals(PartyScreenLogic.TroopType.Member) && side == 0)
            {
                RaiseDeadCampaignBehavior raiseDeadBehavior = Campaign.Current.GetCampaignBehavior<RaiseDeadCampaignBehavior>();
                foreach (TroopRosterElement element in raiseDeadBehavior.TroopsForVM)
                {
                    currentTroopRoster.AddToCounts(element.Character, 1);
                }

                raiseDeadBehavior.TroopsForVM.Clear();
            }
        }
        */
    }
}