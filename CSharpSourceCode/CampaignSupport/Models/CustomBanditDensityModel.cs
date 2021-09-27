using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace TOW_Core.CampaignSupport.Models
{
    public class CustomBanditDensityModel : DefaultBanditDensityModel
    {
        public override int NumberOfMaximumLooterParties
        {
            get
            {
                if(Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 10)
                {
                    return 100;
                }
                else if(Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 20)
                {
                    return 150;
                }
                else
                {
                    return 200;
                }
            }
        }
    }
}
