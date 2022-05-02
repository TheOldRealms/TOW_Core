using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.CampaignSupport
{
    public class TORCustomMobilePartyDialogCampaignBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Start);
        }

        private void Start(CampaignGameStarter obj)
        {
            obj.AddDialogLine("chaos_greeting", "start", "close_window", "Asinine mortal. Prepare to die!", () => EncounteredPartyMatch("chaos_clan_1"), null, 200);
            obj.AddDialogLine("beastmen_greeting", "start", "close_window", "We will trample your puny body beneath our hooves!", () => EncounteredPartyMatch("steppe_bandits"), null, 200);
            obj.AddDialogLine("brokenwheel_greeting", "start", "close_window", "We will break your mind for the glory of Tzeench", () => EncounteredPartyMatch("chs_cult_1"), null, 200);
            obj.AddDialogLine("illumination_greeting", "start", "close_window", "Ascend in death!", () => EncounteredPartyMatch("chs_cult_2"), null, 200);
            obj.AddDialogLine("secondflesh_greeting", "start", "close_window", "Pox consume you!", () => EncounteredPartyMatch("chs_cult_3"), null, 200);
        }

        private bool EncounteredPartyMatch(string clanId)
        {
            if (PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.ActualClan != null)
            {
                return PlayerEncounter.EncounteredMobileParty.ActualClan.StringId == clanId;
            }
            else return false;
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
