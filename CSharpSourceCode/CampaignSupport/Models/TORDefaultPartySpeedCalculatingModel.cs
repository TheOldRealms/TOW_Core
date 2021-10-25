using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartySpeedCalculatingModel : DefaultPartySpeedCalculatingModel
    {
        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            var speed = base.CalculateFinalSpeed(mobileParty, finalSpeed);
            if (mobileParty.Party.Culture.StringId == "khuzait")
            {
                var currentHour = CampaignTime.Now.GetHourOfDay;
                if (currentHour <= 2 || currentHour >= 20)
                {
                    speed.Add(2, new TextObject("Night beings"));
                }
            }
            return speed;
        }
    }
}
