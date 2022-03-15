using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORMarriageModel : DefaultMarriageModel
    {
        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            return false;
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            return false;
        }
    }
}
