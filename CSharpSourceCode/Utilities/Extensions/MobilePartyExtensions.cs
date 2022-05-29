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
                if (party.LeaderHero != null)
                {
                    var engineering = party.LeaderHero.GetSkillValue(DefaultSkills.Engineering);
                    return (int)Math.Truncate((decimal)engineering / 50);
                }
                else return 0;
            }
            else if (party.IsLordParty)
            {
                return 3;
            }
            else return 0;
        }

        public static List<Hero> GetMemberHeroes(this MobileParty party)
        {
            List<Hero> heroes = new List<Hero>();
            foreach(var member in party.MemberRoster.GetTroopRoster())
            {
                if(member.Character.HeroObject != null)
                {
                    heroes.Add(member.Character.HeroObject);
                }
            }
            return heroes;
        }

        public static bool HasSpellCasterMember(this MobileParty party)
        {
            return party.GetMemberHeroes().Any(x => x.IsSpellCaster());
        }

        public static List<Hero> GetSpellCasterMemberHeroes(this MobileParty party)
        {
            return party.GetMemberHeroes().Where(x => x.IsSpellCaster()).ToList();
        }
    }
}
