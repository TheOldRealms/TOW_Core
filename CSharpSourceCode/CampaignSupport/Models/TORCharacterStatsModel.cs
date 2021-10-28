using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORCharacterStatsModel : DefaultCharacterStatsModel
    {
        public override ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false)
        {
            var number = base.MaxHitpoints(character, includeDescriptions);
            number = CalculateHitPoints(number, character);
            return number;
        }

        private ExplainedNumber CalculateHitPoints(ExplainedNumber number, CharacterObject character)
        {
            if (character.IsHero)
            {
                return CalaculateHeroHealth(number, character.HeroObject);
            }
            else
            {
                return CalculateTroopHealth(number, character);
            }
        }

        private ExplainedNumber CalculateTroopHealth(ExplainedNumber number, CharacterObject character)
        {
            switch (character.Tier)
            {
                case 0:
                    number.Add(-15);
                    break;
                case 1:
                case 2:
                case 3:
                    break;
                case 4:
                    number.Add(20);
                    break;
                default:
                    number.Add(character.Tier * 10);
                    break;
            }
            return number;
        }

        private ExplainedNumber CalaculateHeroHealth(ExplainedNumber number, Hero hero)
        {
            var info = hero.GetExtendedInfo();
            if (info != null)
            {
                if (info.AcquiredAttributes.Contains("Tier1"))
                {
                    number.Add(100, new TextObject("Tier1"));
                }
                else if (info.AcquiredAttributes.Contains("Tier2"))
                {
                    number.Add(150, new TextObject("Tier2"));
                }
                else if (info.AcquiredAttributes.Contains("Tier3"))
                {
                    number.Add(200, new TextObject("Tier3"));
                }
                else if (info.AcquiredAttributes.Contains("Tier4"))
                {
                    number.Add(300, new TextObject("Tier4"));
                }
            }
            if (Campaign.Current.CampaignStartTime.IsNow)
            {
                hero.HitPoints = (int)number.ResultNumber;
            }
            return number;
        }
    }
}
