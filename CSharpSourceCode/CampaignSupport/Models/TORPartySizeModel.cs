using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace TOW_Core.CampaignSupport.Models
{
    public class TORPartySizeModel : DefaultPartySizeLimitModel
    {
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            var num = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party.Culture.Name.Contains("Vampire"))
            {
                if (party.Leader != null && party.Leader.IsPlayerCharacter)
                {
                    num.Add(10);
                }
                else
                {
                    num.Add(50);
                }
            }
            return num;
        }
    }
}
