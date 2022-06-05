using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.CustomBattle;
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
            return hero.GetAttributeValue(DefaultCharacterAttributes.Intelligence) * 0.07f;
        }

        public static HeroExtendedInfo GetExtendedInfo(this Hero hero)
        {
            return ExtendedInfoManager.Instance.GetHeroInfoFor(hero.GetInfoKey());
        }

        public static int GetPlaceableArtilleryCount(this Hero hero)
        {
            int count = 0;
            if (hero.CanPlaceArtillery())
            {
                var engineering = hero.GetSkillValue(DefaultSkills.Engineering);
                count = (int)Math.Truncate((decimal)engineering / 50);
            }
            return count;
        }

        public static bool CanPlaceArtillery(this Hero hero)
        {
            return hero.HasAttribute("CanPlaceArtillery");
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

        public static void AddKnownLore(this Hero hero, string loreID)
        {
            if (hero.GetExtendedInfo() != null)
            {
                hero.GetExtendedInfo().AddKnownLore(loreID);
            }
        }

        public static bool HasKnownLore(this Hero hero, string loreID)
        {
            if (hero.GetExtendedInfo() != null)
            {
                return hero.GetExtendedInfo().HasKnownLore(loreID);
            }
            else return false;
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

        /// <summary>
        /// Returns the key which should be used for keying heroes in dictionaries
        /// 
        /// <br>Format is as follows:</br>
        /// <br>StringId_Occupation_Name</br>
        /// </summary>
        /// <param name="hero"></param>
        public static String GetInfoKey(this Hero hero)
        {
            return hero.StringId;
            //return String.Format("{0}_{1}_{2}", hero.StringId, hero.Occupation.ToString(), hero.Name);
        }

        public static void ReinitializeHero(this Hero hero)
        {
            CharacterDevelopmentCampaignBehavior characterDeveloper
                = Campaign.Current.GetCampaignBehavior<CharacterDevelopmentCampaignBehavior>();

            hero.ClearAttributes();
            int UnspentAttributePoints = 
                (hero.Level - 1)
                / Campaign.Current.Models.CharacterDevelopmentModel.LevelsPerAttributePoint
                + Campaign.Current.Models.CharacterDevelopmentModel.AttributePointsAtStart;
            hero.HeroDeveloper.UnspentAttributePoints = UnspentAttributePoints;

            foreach (CharacterAttribute attribute in Attributes.All)
            {
                hero.HeroDeveloper.AddAttribute(attribute, 1, false);
                UnspentAttributePoints -= 1;
            }
            characterDeveloper.DevelopCharacterStats(hero);
        }
    }
}