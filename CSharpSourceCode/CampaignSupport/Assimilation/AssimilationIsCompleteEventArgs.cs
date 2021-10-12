using TaleWorlds.CampaignSystem;

namespace TOW_Core.CampaignSupport.Assimilation
{
    public class AssimilationIsCompleteEventArgs
    {
        public AssimilationIsCompleteEventArgs(Settlement settlement, CultureObject culture)
        {
            this._settlement = settlement;
            this._culture = culture;
        }

        public Settlement Settlement { get => _settlement; }

        public CultureObject Culture { get => _culture; }

        private Settlement _settlement;

        private CultureObject _culture;
    }
}
