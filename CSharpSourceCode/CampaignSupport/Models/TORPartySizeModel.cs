using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Localization;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartySizeModel : DefaultPartySizeLimitModel
    {
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            var num = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party.Culture.Name.Contains("Vampire"))
            {
                if (party.LeaderHero != null && party.LeaderHero.IsHumanPlayerCharacter)
                {
                    num.Add(20, new TextObject("Friend of undead"));
                }
                else if (party.Id.Contains("caravan"))
                {
                    num.Add(50, new TextObject("Caravan of death"));
                }
                else if (party.IsSettlement)
                {
                    if (party.Settlement.IsVillage)
                    {
                        num.Add(50, new TextObject("Settlement of Vampire Counts"));
                    }
                    else
                    {
                        num.Add(300, new TextObject("Settlement of Vampire Counts"));
                    }
                }
                else if (party.IsMobile)
                {
                    num.Add(100, new TextObject("Vampire lord"));
                }
            }
            return num;
        }
    }
}
