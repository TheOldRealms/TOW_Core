using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class SettlementCultureChangedNotificationItemVM : MapNotificationItemBaseVM
    {
        public SettlementCultureChangedNotificationItemVM(SettlementCultureChangedMapNotification data) : base(data)
        {
            this._settlement = data.ConvertedSettlement;
            this._culture = data.NewCulture;
            base.NotificationIdentifier = "settlementownerchanged";
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEvents.OnSettlementOwnerChangedEvent.ClearListeners(this);
        }


        private Settlement _settlement;

        private CultureObject _culture;
    }
}
