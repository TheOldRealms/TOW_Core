using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class SettlementCultureChangedLogEntry : LogEntry
    {
        public SettlementCultureChangedLogEntry(Settlement settlement, CultureObject culture)
        {
            this.ConvertedSettlement = settlement;
            this.NewCulture = culture;
        }

        public override CampaignTime KeepInHistoryTime
        {
            get
            {
                return CampaignTime.Weeks(1f);
            }
        }

        [SaveableField(10)]
        public readonly Settlement ConvertedSettlement;

        [SaveableField(20)]
        public readonly CultureObject NewCulture;
    }
}
