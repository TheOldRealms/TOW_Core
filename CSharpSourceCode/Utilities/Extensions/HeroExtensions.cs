using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TOW_Core.Abilities.SpellBook;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Utilities.Extensions
{
    public static class HeroExtensions
    {
        public static bool CanRaiseDead(this Hero hero)
        {
            return hero.IsHumanPlayerCharacter && hero.IsNecromancer();
        }

        /// <summary>
        /// Returns raise dead chance, where, for example, 0.1 is a 10% chance.
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        public static float GetRaiseDeadChance(this Hero hero)
        {
            return 0.1f;
        }

        public static HeroExtendedInfo GetExtendedInfo(this Hero hero)
        {
            return ExtendedInfoManager.Instance.GetHeroInfoFor(hero.StringId);
            /*
            var info = Campaign.Current?.GetCampaignBehavior<ExtendedInfoManager>();
            if (info != null)
            {
                return info.GetHeroInfoFor(hero.StringId);
            }
            else return null;
            */
        }

        public static void AddAbility(this Hero hero, string ability)
        {
            var info = hero.GetExtendedInfo();
            if (info != null && !info.AllAbilities.Contains(ability))
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
            if (hero.GetExtendedInfo() != null)
            {
                return hero.GetExtendedInfo().AllAttributes.Contains(attribute);
            }
            else return false;
        }

        public static bool HasAbility(this Hero hero, string ability)
        {
            if (hero.GetExtendedInfo() != null)
            {
                return hero.GetExtendedInfo().AllAbilities.Contains(ability);
            }
            else return false;
        }

        public static void SetSpellCastingLevel(this Hero hero, SpellCastingLevel level)
        {
            if (hero.GetExtendedInfo() != null)
            {
                hero.GetExtendedInfo().SpellCastingLevel = level;
            }
        }

        public static bool IsSpellCaster(this Hero hero)
        {
            return hero.HasAttribute("SpellCaster");
        }

        public static bool IsAbilityUser(this Hero hero)
        {
            return hero.HasAttribute("AbilityUser");
        }

        public static bool IsNecromancer(this Hero hero)
        {
            return hero.HasAttribute("Necromancer");
        }

        public static bool IsUndead(this Hero hero)
        {
            return hero.HasAttribute("Undead");
        }

        public static bool IsVampire(this Hero hero)
        {
            return hero.HasAttribute("VampireBodyOverride");
        }

        public static bool IsSuitableForSettlement(this Hero hero, Settlement settlement)
        {
            if (settlement.IsVampireSettlement())
            {
                return hero.Culture.Name.Contains("Vampire");
            }
            else
            {
                return hero.Culture.Name.Contains("Empire");
            }
        }

        public static bool IsOutrider(this Hero hero, CultureObject culture)
        {
            return hero.Culture != culture;
        }
    }
}