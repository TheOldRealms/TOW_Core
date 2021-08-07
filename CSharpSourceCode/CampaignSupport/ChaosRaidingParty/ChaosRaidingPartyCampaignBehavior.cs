using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TOW_Core.CampaignSupport.QuestBattleLocation;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport.ChaosRaidingParty
{
    public class ChaosRaidingPartyCampaignBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
            CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, HourlyTickPartyAI);
        }

        private void HourlyTickPartyAI(MobileParty party, PartyThinkParams partyThinkParams)
        {
            if (party.PartyComponent is ChaosRaidingPartyComponent)
            {
                var component = (ChaosRaidingPartyComponent) party.PartyComponent;

                if (component.Patrol)
                {
                    PatrolBehavior(partyThinkParams, component);
                }

                if (!component.Patrol)
                {
                    RaiderBehavior(party, partyThinkParams, component);
                }

                party.Ai.SetDoNotMakeNewDecisions(false);
            }
        }

        private static void RaiderBehavior(MobileParty party, PartyThinkParams partyThinkParams, ChaosRaidingPartyComponent component)
        {
            var scores = partyThinkParams.AIBehaviorScores.ToList().FindAll(pair => pair.Value >= 9.9f);
            if (party.TargetSettlement != null && party.TargetSettlement.IsRaided)
            {
                scores.ForEach(pair => partyThinkParams.AIBehaviorScores[pair.Key] = 0.0f);
                var find = FindAllBelongingToSettlement("Averheim").FindAll(settlementF => !settlementF.IsRaided);
                if (find.Count != 0)
                {
                    partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(find.GetRandomElement(), AiBehavior.RaidSettlement)] = 10f;
                }
                else
                {
                    partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(component.Portal, AiBehavior.GoToSettlement)] = 8f;
            //        party.Ai.SetAIState(AIState.VisitingVillage);
           //         party.SetMoveGoToSettlement(component.Portal);
                }
            }
            else if (party.TargetSettlement != null && party.TargetSettlement != component.Portal)
            {
                partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(party.TargetSettlement, AiBehavior.RaidSettlement)] = 10f;
            }
            
            if (party.TargetSettlement == null && party.TargetSettlement != component.Portal)
            {
                partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(party.TargetSettlement, AiBehavior.GoToSettlement)] = 8f;
          //      party.Ai.SetAIState(AIState.VisitingVillage);
           //     party.SetMoveGoToSettlement(component.Portal);
            }

            if (party.TargetSettlement == null)
            {
                if (scores.Count == 0)
                {
                    var find = FindAllBelongingToSettlement("Averheim").FindAll(settlementF => !settlementF.IsRaided);
                    if (find.Count != 0)
                    {
                        partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(find.GetRandomElement(), AiBehavior.RaidSettlement)] = 10f;
                    }
                }
            }
        }

        private static void PatrolBehavior(PartyThinkParams partyThinkParams, ChaosRaidingPartyComponent component)
        {
            AIBehaviorTuple key = new AIBehaviorTuple(component.Portal, AiBehavior.PatrolAroundPoint);
            partyThinkParams.AIBehaviorScores[key] = 10f;
          //  component.Party.MobileParty.Ai.SetAIState(AIState.Raiding);
          //  component.Party.MobileParty.SetMoveRaidSettlement(find);
        }

        private void DailyTickSettlement(Settlement settlement)
        {
            if (settlement.Name.ToString() != "Chaos Portal") return; //TODO: Is there a better way to do this?
            var questBattleComponent = settlement.GetComponent<QuestBattleComponent>();
            SpawnNewParties(settlement, questBattleComponent);
        }

        private static void SpawnNewParties(Settlement settlement, QuestBattleComponent questBattleComponent)
        {
            if (questBattleComponent != null)
            {
                if (questBattleComponent.RaidingParties.Count < 5)
                {
                    FindAllBelongingToSettlement("Averheim").GetRandomElement();
                    ChaosRaidingPartyComponent.CreateChaosRaidingParty("chaos_clan_1_party_" + questBattleComponent.RaidingParties.Count + 1, settlement, questBattleComponent, TOWMath.GetRandomInt(35, 70));
                }

                if (questBattleComponent.PatrolParties.Count < 2)
                {
                    ChaosRaidingPartyComponent.CreateChaosPatrolParty("chaos_clan_1_patrol_" + questBattleComponent.PatrolParties.Count + 1, settlement, questBattleComponent, TOWMath.GetRandomInt(105, 135));
                }
            }
        }

        private static List<Settlement> FindAllBelongingToSettlement(string settlementName)
        {
            return Campaign.Current.Settlements.ToList().FindAll(settlementF => settlementF.IsVillage && settlementF.Village.Bound.Name.ToString() == settlementName);
        }
    }
}