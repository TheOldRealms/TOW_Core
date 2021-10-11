using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TOW_Core.CampaignSupport.SettlementComponents;

namespace TOW_Core.CampaignSupport.CampaignBehaviors
{
    public class AssimilationCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinishedEvent);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(OnSettlementOwnerChanged));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(OnDailyTick));
        }

        private void OnGameLoadFinishedEvent()
        {
            foreach (var comp in _assimilationComponents)
            {
                Traverse.Create(comp.Settlement).Field("_settlementComponents").GetValue<List<SettlementComponent>>().Add(comp);
                comp.SetParameters(comp.Settlement);
            }
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (settlement.Culture != newOwner.MapFaction.Culture && (settlement.IsCastle || settlement.IsTown))
            {
                var component = settlement.GetComponent<AssimilationComponent>();
                if (component == null)
                {
                    component = settlement.AddComponent<AssimilationComponent>();
                    _assimilationComponents.Add(component);
                    component.SetParameters(settlement);
                }
                else
                {
                    component.SetParameters(settlement);
                }
            }
        }

        private void OnDailyTick()
        {
            foreach (var component in _assimilationComponents)
            {
                if (component != null && !component.IsAssimilationComplete)
                {
                    component.Tick();
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<AssimilationComponent>>("_assimilationComponents", ref _assimilationComponents);
        }

        private List<AssimilationComponent> _assimilationComponents = new List<AssimilationComponent>();
    }
}
