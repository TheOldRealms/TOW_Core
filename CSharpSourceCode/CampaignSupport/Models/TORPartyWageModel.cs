using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartyWageModel : DefaultPartyWageModel
    {
        public override int GetCharacterWage(int tier)
        {
            switch (tier)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 5;
                case 4:
                    return 8;
                case 5:
                    return 12;
                case 6:
                    return 17;
                case 7:
                    return 23;
                case 8:
                    return 30;
                default:
                    return 40;
            }
        }
    }
}
