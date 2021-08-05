using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.CampaignSupport.PartyComponent;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class ChaosPartyCampaignBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
        }

        private void DailyTickSettlement(Settlement settlement)
        {
            if (settlement.Name.ToString() != "Chaos Portal") return; //TODO: Is there a better way to do this?
            var questBattleComponent = settlement.GetComponent<QuestBattleComponent>();
            SpawnNewParties(settlement, questBattleComponent);
            ManagePartyGoals(settlement, questBattleComponent);
        }

        private void ManagePartyGoals(Settlement settlement, QuestBattleComponent questBattleComponent)
        {
            foreach (var chaosRaidingPartyComponent in questBattleComponent.PatrolParties)
            {
                var chaosRaidingParty = chaosRaidingPartyComponent.MobileParty;
                if (chaosRaidingParty.TargetSettlement.IsRaided)
                {
                    var find = FindAllBelongingToSettlement("Averheim").FindAll(settlementF => !settlementF.IsRaided);
                    if (find.Count > 0)
                    {
                        chaosRaidingParty.Ai.SetAIState(AIState.Raiding);
                        chaosRaidingParty.SetMoveRaidSettlement(find.GetRandomElement());
                        chaosRaidingParty.Ai.SetDoNotMakeNewDecisions(true);
                    }
                    else
                    {
                        chaosRaidingParty.Ai.SetAIState(AIState.VisitingVillage);
                        chaosRaidingParty.SetMovePatrolAroundSettlement(settlement);
                    }
                }
            }
        }

        private static void SpawnNewParties(Settlement settlement, QuestBattleComponent questBattleComponent)
        {
            if (questBattleComponent != null)
            {
                if (questBattleComponent.RaidingParties.Count < 5)
                {
                    var find = FindAllBelongingToSettlement("Averheim").GetRandomElement();
                    var chaosRaidingParty = ChaosRaidingPartyComponent.CreateChaosRaidingParty("chaos_clan_1_party_" + questBattleComponent.RaidingParties.Count + 1, settlement, questBattleComponent, 30);
                    chaosRaidingParty.Ai.SetAIState(AIState.Raiding);
                    chaosRaidingParty.SetMoveRaidSettlement(find);
                    chaosRaidingParty.Ai.SetDoNotMakeNewDecisions(true);
                    FactionManager.DeclareWar(chaosRaidingParty.Party.MapFaction, Clan.PlayerClan);
                    TOWCommon.Say("Raiding " + find.Name);
                }

                if (questBattleComponent.PatrolParties.Count < 2)
                {
                    var chaosRaidingParty = ChaosRaidingPartyComponent.CreateChaosPatrolParty("chaos_clan_1_patrol_" + questBattleComponent.PatrolParties.Count + 1, settlement, questBattleComponent, 120);
                    chaosRaidingParty.Ai.SetAIState(AIState.PatrollingAroundLocation);
                    chaosRaidingParty.SetMovePatrolAroundSettlement(settlement);
                    TOWCommon.Say("Patrolling around " + settlement.Name);
                }
            }
        }

        private static List<Settlement> FindAllBelongingToSettlement(string settlementName)
        {
            return Campaign.Current.Settlements.ToList().FindAll(settlementF => settlementF.IsVillage && settlementF.Village.Bound.Name.ToString() == settlementName);
        }
    }
}