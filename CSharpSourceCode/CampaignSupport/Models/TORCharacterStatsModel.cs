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
            var num = base.MaxHitpoints(character, includeDescriptions);
            if (character.IsPlayerCharacter)
            {
                var info = Hero.MainHero.GetExtendedInfo();
                if (Campaign.Current.CampaignBehaviorManager != null && info != null && info.AcquiredAttributes.Contains("VampireBodyOverride"))
                {
                    num.Add(100, new TextObject("Vampire body"));
                    if (character.HeroObject != null && Campaign.Current.CampaignStartTime.IsNow)
                    {
                        character.HeroObject.HitPoints = (int)num.ResultNumber;
                    }
                }
            }
            else if (character.IsVampire())
            {
                num.Add(100);
            }
            return num;
        }
    }
}
