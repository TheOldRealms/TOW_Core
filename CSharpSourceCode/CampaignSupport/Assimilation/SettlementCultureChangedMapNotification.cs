using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class SettlementCultureChangedMapNotification : InformationData
    {
        public SettlementCultureChangedMapNotification(Settlement settlement, CultureObject culture, TextObject description) : base(description)
        {
            this.ConvertedSettlement = settlement;
            this.NewCulture = culture;
        }


        [SaveableProperty(3)]
        public Settlement ConvertedSettlement { get; private set; }

        [SaveableProperty(4)]
        public CultureObject NewCulture { get; private set; }

        public override TextObject TitleText
        {
            get
            {
                return new TextObject("{=3NCExCi1}Area Culture Converted", null);
            }
        }

        public override string SoundEventPath
        {
            get
            {
                return "event:/ui/notification/peace";
            }
        }
    }
}
