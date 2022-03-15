using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TOW_Core.ObjectDataExtensions;

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

        public static MobilePartyExtendedInfo GetInfo(this MobileParty party)
        {
            var manager = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if (manager != null)
            {
                return manager.GetPartyInfoFor(party.StringId);
            }
            else return null;
        }

        public static List<ItemRosterElement> GetArtilleryItems(this MobileParty party)
        {
            List<ItemRosterElement> list = new List<ItemRosterElement>();
            list.AddRange(party.ItemRoster.Where(x => x.EquipmentElement.Item.StringId.Contains("artillery")).ToList());
            return list;
        }

        public static int GetMaxNumberOfArtillery(this MobileParty party)
        {
            if (party == MobileParty.MainParty)
            {
                var engineering = party.LeaderHero.GetSkillValue(DefaultSkills.Engineering);
                return (int)Math.Truncate((decimal)engineering / 50);
            }
            else if (party.IsLordParty)
            {
                return 3;
            }
            else return 0;
        }
    }
}
