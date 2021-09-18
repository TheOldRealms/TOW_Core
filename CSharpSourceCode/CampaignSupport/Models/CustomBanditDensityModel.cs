using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace TOW_Core.CampaignSupport.Models
{
    public class CustomBanditDensityModel : DefaultBanditDensityModel
    {
        public override int NumberOfMaximumLooterParties
        {
            get
            {
                return 100;
            }
        }
    }
}
