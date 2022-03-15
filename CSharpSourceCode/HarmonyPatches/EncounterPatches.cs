using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
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
    }
}
