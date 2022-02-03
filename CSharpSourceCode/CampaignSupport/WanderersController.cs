using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class WanderersController : CampaignBehaviorBase
    {
        private List<Hero> _companions;
        private Dictionary<Settlement, CampaignTime> _companionSettlements;

        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnAfterSettlementEntered);
        }

        private void OnAfterSessionLaunched(CampaignGameStarter gameStarter)
        {
            var urbanBehavior = Campaign.Current.GetCampaignBehavior<UrbanCharactersCampaignBehavior>();
            if (urbanBehavior != null)
            {
                _companions = Traverse.Create(urbanBehavior).Field("_companions").GetValue<List<Hero>>();
                _companionSettlements = Traverse.Create(urbanBehavior).Field("_companionSettlements").GetValue<Dictionary<Settlement, CampaignTime>>();
            }
        }

        private void OnAfterSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (hero == null || !hero.IsHumanPlayerCharacter)
            {
                return;
            }

            var wanderer = Hero.AllAliveHeroes.FirstOrDefault(h => h.IsWanderer && h.CurrentSettlement == settlement);

            //check for unsuitable wanderer
            if (wanderer != null && !wanderer.IsSuitableForSettlement(settlement))
            {
                //look for empty suitable settlement to move unsuitable wanderer
                var emptySuitableSettlement = Settlement.All.Except(_companionSettlements.Keys).FirstOrDefault(s => s.IsTown && s.Culture == wanderer.Culture);
                if (emptySuitableSettlement != null)
                {
                    //move to other settlement
                    MoveWanderer(wanderer, emptySuitableSettlement);
                }
                else
                {
                    //move to list of companions
                    LeaveSettlementAction.ApplyForCharacterOnly(wanderer);
                    wanderer.ChangeState(Hero.CharacterStates.NotSpawned);
                    _companions.Add(wanderer);
                }

                //look for new suitable wanderer
                Hero newWanderer = _companions.GetRandomElementWithPredicate((Hero h) => h.IsSuitableForSettlement(settlement));
                if (newWanderer != null)
                {
                    newWanderer.ChangeState(Hero.CharacterStates.Active);
                    MoveWanderer(newWanderer, settlement);
                }
            }
        }

        private void MoveWanderer(Hero wanderer, Settlement settlement)
        {
            EnterSettlementAction.ApplyForCharacterOnly(wanderer, settlement);
            if (_companionSettlements.ContainsKey(settlement))
            {
                _companionSettlements.Remove(settlement);
            }
            _companionSettlements.Add(settlement, CampaignTime.Now);
            _companions.Remove(wanderer);
        }
    }
}
