using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.Utilities.Extensions
{
    public static class MobilePartyExtensions
    {
        public static bool IsNearASettlement(this MobileParty party, float threshold = 1.5f)
        {
            foreach (Settlement settlement in Settlement.All)
            {
                float distance;
                Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, party, Campaign.MapDiagonal, out distance);
                if (distance < threshold)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
