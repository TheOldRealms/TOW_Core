using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORBanditDensityModel : DefaultBanditDensityModel
    {
        public override int NumberOfMaximumLooterParties
        {
            get
            {
                if (Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 10)
                {
                    return 100;
                }
                else if(Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 20)
                {
                    return 200;
                }
                else
                {
                    return 300;
                }
            }
        }

        public override int NumberOfMaximumBanditPartiesAroundEachHideout => 5;
        public override int NumberOfMaximumBanditPartiesInEachHideout => 3;
        public override int NumberOfInitialHideoutsAtEachBanditFaction => 10;
        public override int NumberOfMaximumHideoutsAtEachBanditFaction => 19;
    }
}
