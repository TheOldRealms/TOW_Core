using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TOW_Core.CampaignSupport.BattleHistory;
using TOW_Core.CampaignSupport.QuestBattleLocation;
using TOW_Core.CampaignSupport.RaiseDead;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class EncounterPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerEncounter), "StartBattleInternal")]
        public static void Postfix(ref MapEvent __result, PartyBase ____defenderParty, ref MapEvent ____mapEvent, PartyBase ____attackerParty)
        {
            if(__result == null && ____defenderParty.IsSettlement && ____defenderParty.Settlement != null)
            {
                var comp = ____defenderParty.Settlement.GetComponent<QuestBattleComponent>();
                if(comp != null)
                {
                    ____mapEvent = Campaign.Current.MapEventManager.StartBattleMapEvent(____attackerParty, comp.QuestOpponentParty.Party);
                    __result = ____mapEvent;
                }
            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerEncounter), "DoLootParty")]
        public static void GenerateRaiseDeadTroops()
        {
            RaiseDeadCampaignBehavior raiseDeadBehavior = Campaign.Current.GetCampaignBehavior<RaiseDeadCampaignBehavior>();
            raiseDeadBehavior.TroopsForVM = raiseDeadBehavior.GenerateRaisedTroopsForVM();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerEncounter), "DoLootParty")]
        public static void ShowPartyScreenIfRaiseDeadIsPossible(ref bool ____stateHandled)
        {
            TroopRoster memberRosterReceivingLootShare = MobileParty.MainParty.MapEventSide.MemberRosterForPlayerLootShare(MobileParty.MainParty.Party);
            TroopRoster prisonerRosterReceivingLootShare = MobileParty.MainParty.MapEventSide.PrisonerRosterForPlayerLootShare(MobileParty.MainParty.Party);

            RaiseDeadCampaignBehavior raiseDeadBehavior = Campaign.Current.GetCampaignBehavior<RaiseDeadCampaignBehavior>();

            //____stateHandled is true if the screen is already going to be opened, so don't do it again
            //Raise dead troop generation is random, so it's possible no troops were raised, so we check the count to avoid opening an empty party screen.
            bool partyScreenShouldOpen = Hero.MainHero.CanRaiseDead() && !____stateHandled && raiseDeadBehavior.TroopsForVM.Count > 0;
            if (partyScreenShouldOpen)
            {
                ____stateHandled = true;
                PartyScreenManager.OpenScreenAsLoot(memberRosterReceivingLootShare, prisonerRosterReceivingLootShare, TextObject.Empty, memberRosterReceivingLootShare.TotalManCount + prisonerRosterReceivingLootShare.TotalManCount, null);
            }
        }
        */
    }
}
