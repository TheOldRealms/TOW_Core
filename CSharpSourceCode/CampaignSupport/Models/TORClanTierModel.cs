using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

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
