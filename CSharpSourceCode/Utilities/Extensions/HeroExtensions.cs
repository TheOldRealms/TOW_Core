using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Utilities.Extensions
{
    public static class HeroExtensions
    {
        public static HeroExtendedInfo GetExtendedInfo(this Hero hero)
        {
            var info = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if (info != null)
            {
                return info.GetHeroInfoFor(hero.StringId);
            }
            else return null;
        }

        public static void AddAbility(this Hero hero, string ability)
        {
            var info = hero.GetExtendedInfo();
            if(info != null && !info.AllAbilities.Contains(ability))
            {
                info.AcquiredAbilities.Add(ability);
            }
        }

        public static void AddAttribute(this Hero hero, string attribute)
        {
            var info = hero.GetExtendedInfo();
            if (info != null && !info.AllAttributes.Contains(attribute))
            {
                info.AcquiredAttributes.Add(attribute);
            }
        }

        public static bool HasAttribute(this Hero hero, string attribute)
        {
            return hero.GetExtendedInfo().AllAttributes.Contains(attribute);
        }

        public static bool HasAbility(this Hero hero, string ability)
        {
            return hero.GetExtendedInfo().AllAbilities.Contains(ability);
        }

        public static bool IsSpellCaster(this Hero hero)
        {
            return hero.HasAttribute("SpellCaster");
        }
    }
}
