using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace TOW_Core.CampaignSupport.Models
{
    class TORClanTierModel : DefaultClanTierModel
    {
        public override int GetPartyLimitForTier(Clan clan, int clanTierToCheck)
        {
            // Patch AI clans to have unlimited parties so that new lords aren't
            // spawned without a party.
            return Clan.PlayerClan.Equals(clan)
                ? base.GetPartyLimitForTier(clan, clanTierToCheck)
                : clan.Heroes.Count;
        }
    }
}
