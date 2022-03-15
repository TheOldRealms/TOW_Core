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
                if(Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 10)
                {
                    return 50;
                }
                else if(Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 20)
                {
                    return 100;
                }
                else
                {
                    return 150;
                }
            }
        }
    }
}
