using SandBox.View.Map;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class AssimilationCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinishedEvent);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnGameOverEvent.AddNonSerializedListener(this, OnGameOver);
        }

        private void OnGameOver()
        {
            for (int i = 0; i < _assimilationComponents.Count; i++)
            {
                _assimilationComponents[i].AssimilationIsComplete -= ShowMapNotification;
            }
        }

        private void OnGameLoadFinishedEvent()
        {
            MapScreen.Instance.MapNotificationView.RegisterMapNotificationType(typeof(SettlementCultureChangedMapNotification), typeof(SettlementCultureChangedNotificationItemVM));
            foreach (var component in _assimilationComponents)
            {
                component.InitializeComponent(false);
                component.AssimilationIsComplete += ShowMapNotification;
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
                    component.InitializeComponent(true);
                    _assimilationComponents.Add(component);
                    component.AssimilationIsComplete += ShowMapNotification;
                }
                else
                {
                    component.StartNewAssimilation();
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

        private void ShowMapNotification(object obj, AssimilationIsCompleteEventArgs e)
        {
            LogEntry.AddLogEntry(new SettlementCultureChangedLogEntry(e.Settlement, e.Culture));
            var description = new TextObject($"{e.Settlement.Name} has converted to {e.Culture.Name}");
            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new SettlementCultureChangedMapNotification(e.Settlement, e.Culture, description));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<AssimilationComponent>>("_assimilationComponents", ref _assimilationComponents);
        }


        private List<AssimilationComponent> _assimilationComponents = new List<AssimilationComponent>();
    }
}
