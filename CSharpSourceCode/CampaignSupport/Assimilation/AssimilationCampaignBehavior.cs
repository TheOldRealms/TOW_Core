using HarmonyLib;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Assimilation
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
            MapScreen.Instance.MapNotificationView.RegisterMapNotificationType(typeof(SettlementCultureChangedMapNotification), typeof(SettlementCultureChangedNotificationItemVM));
            foreach (var component in _assimilationComponents)
            {
                Traverse.Create(component.Settlement).Field("_settlementComponents").GetValue<List<SettlementComponent>>().Add(component);
                component.SetParameters(component.Settlement);
                component.AssimilationIsComplete += (o, e) => ShowMapNotification(e.Settlement, e.Culture);
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
                    component.AssimilationIsComplete += (o, e) => ShowMapNotification(e.Settlement, e.Culture);
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

        private void ShowMapNotification(Settlement settlement, CultureObject culture)
        {
            LogEntry.AddLogEntry(new SettlementCultureChangedLogEntry(settlement, culture));
            var description = new TextObject($"Culture of {settlement.Name} area converted to {culture.Name}");
            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new SettlementCultureChangedMapNotification(settlement, culture, description));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<AssimilationComponent>>("_assimilationComponents", ref _assimilationComponents);
        }


        private List<AssimilationComponent> _assimilationComponents = new List<AssimilationComponent>();
    }
}
