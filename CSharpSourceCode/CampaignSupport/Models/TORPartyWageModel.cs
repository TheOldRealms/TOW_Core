using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartyWageModel : DefaultPartyWageModel
    {
        public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            if (troop.IsUndead())
            {
                return 0;
            }
            return base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
        }
    }
}
