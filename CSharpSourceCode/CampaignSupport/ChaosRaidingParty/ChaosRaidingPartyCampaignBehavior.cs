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
            if (component.Target == null || component.Target.IsRaided || component.Target == component.Portal)
            {
                var find = FindAllBelongingToSettlement("Averheim").FindAll(settlementF => !settlementF.IsRaided);
                if (find.Count != 0)
                {
                    component.Target = find.GetRandomElement();
                }
                else
                {
                    component.Target = component.Portal;
                }
            }

            if (component.Target.IsVillage && !component.Target.IsRaided && component.Target != component.Portal)
            {
                partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(component.Target, AiBehavior.RaidSettlement)] = 10f;
            }

            if (component.Target == component.Portal)
            {
                partyThinkParams.AIBehaviorScores[new AIBehaviorTuple(component.Portal, AiBehavior.GoToSettlement)] = 8f;
            }
        }

        private static void PatrolBehavior(PartyThinkParams partyThinkParams, ChaosRaidingPartyComponent component)
        {
            AIBehaviorTuple key = new AIBehaviorTuple(component.Portal, AiBehavior.PatrolAroundPoint);
            partyThinkParams.AIBehaviorScores[key] = 10f;
        }

        private void DailyTickSettlement(Settlement settlement)
        {
            if (settlement.Name.ToString() != "Chaos Portal") return;
            var questBattleComponent = settlement.GetComponent<QuestBattleComponent>();
            SpawnNewParties(settlement, questBattleComponent);
        }

        private static void SpawnNewParties(Settlement settlement, QuestBattleComponent questBattleComponent)
        {
            if (questBattleComponent != null)
            {
                if (questBattleComponent.RaidingParties.Count < 5)
                {
                    var find = FindAllBelongingToSettlement("Averheim").GetRandomElement();
                    var chaosRaidingParty = ChaosRaidingPartyComponent.CreateChaosRaidingParty("chaos_clan_1_party_" + questBattleComponent.RaidingParties.Count + 1, settlement, questBattleComponent, TOWMath.GetRandomInt(35, 70));
                    chaosRaidingParty.Ai.SetAIState(AIState.Raiding);
                    chaosRaidingParty.SetMoveRaidSettlement(find);
                    ((ChaosRaidingPartyComponent) chaosRaidingParty.PartyComponent).Target = find;
                    if (!chaosRaidingParty.Party.MapFaction.IsAtWarWith(Clan.PlayerClan))
                    {
                        FactionManager.DeclareWar(chaosRaidingParty.Party.MapFaction, Clan.PlayerClan);
                    }
                }

                if (questBattleComponent.PatrolParties.Count < 2)
                {
                    var chaosRaidingParty = ChaosRaidingPartyComponent.CreateChaosPatrolParty("chaos_clan_1_patrol_" + questBattleComponent.PatrolParties.Count + 1, settlement, questBattleComponent, TOWMath.GetRandomInt(105, 135));
                    chaosRaidingParty.Ai.SetAIState(AIState.PatrollingAroundLocation);
                    chaosRaidingParty.SetMovePatrolAroundSettlement(settlement);
                }
            }
        }

        private static List<Settlement> FindAllBelongingToSettlement(string settlementName)
        {
            return Campaign.Current.Settlements.ToList().FindAll(settlementF => settlementF.IsVillage && settlementF.Village.Bound.Name.ToString() == settlementName);
        }
    }
}