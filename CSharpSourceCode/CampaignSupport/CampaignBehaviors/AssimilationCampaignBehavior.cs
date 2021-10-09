using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TOW_Core.CampaignSupport.SettlementComponents;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport.CampaignBehaviors
{
    public class AssimilationCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(OnOwnerChanged));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(CheckForAssimilationComponent));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinishedEvent);

            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(DEBUGOnSettlementEntered));
        }

        private void OnGameLoadFinishedEvent()
        {
            foreach (var comp in _assimilationComponents)
            {
                Traverse.Create(comp.Settlement).Field("_settlementComponents").GetValue<List<SettlementComponent>>().Add(comp);
                comp.SetParameters(comp.Settlement);
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
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

        private void CheckForAssimilationComponent()
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




        private void DEBUGOnSettlementEntered(MobileParty arg1, Settlement arg2, Hero arg3)
        {
            if (arg3 != null && arg3.CharacterObject != null && arg3.CharacterObject.IsPlayerCharacter)
            {
                if (arg2.Owner != arg3)
                {
                    ChangeOwnerOfSettlementAction.ApplyByGift(arg2, arg3);
                }
                else
                {
                    var comp = arg2.GetComponent<AssimilationComponent>();
                    if (comp != null)
                    {
                        TOWCommon.Say($"{comp.Settlement.Name} {comp.InitialOutriderAmount} {comp.Outriders.Count} {comp.AssimilationProgress}");
                    }
                    else
                    {
                        TOWCommon.Say("There is no component");
                    }
                }
            }
        }
    }
}
