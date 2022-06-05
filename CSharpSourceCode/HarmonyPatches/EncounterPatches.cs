using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.CampaignSupport.QuestBattleLocation;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerEncounter), "Init", typeof(PartyBase), typeof(PartyBase), typeof(Settlement))]
        public static void Postfix2(PartyBase attackerParty, PartyBase defenderParty, Settlement settlement = null)
        {
            if (defenderParty.MapEvent != null && settlement != null && defenderParty != MobileParty.MainParty.Party && attackerParty == MobileParty.MainParty.Party)
            {
                var mapEvent = defenderParty.MapEvent;
                if (MapEventHelper.CanJoinBattle(MobileParty.MainParty, mapEvent, BattleSideEnum.Defender))
                {
                    MobileParty.MainParty.Party.MapEventSide = mapEvent.DefenderSide;
                }
                else if (MapEventHelper.CanJoinBattle(MobileParty.MainParty, mapEvent, BattleSideEnum.Attacker))
                {
                    MobileParty.MainParty.Party.MapEventSide = mapEvent.AttackerSide;
                }
            }
        }
    }
}
