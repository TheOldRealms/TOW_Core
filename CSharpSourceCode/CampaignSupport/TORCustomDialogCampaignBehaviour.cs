using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class TORCustomDialogCampaignBehaviour : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Start);
        }

        private void Start(CampaignGameStarter obj)
        {
            obj.AddDialogLine("chaos_greeting", "start", "close_window", "Asinine mortal. Prepare to die!", () => EncounteredPartyMatch("chaos_clan_1")&&!HeroIsWounded(), null, 200);
            obj.AddDialogLine("chaos_die", "start", "close_window", "I will return!", () => EncounteredPartyMatch("chaos_clan_1")&&HeroIsWounded(), null, 200);

            obj.AddDialogLine("beastmen_greeting", "start", "close_window", "We will trample your puny body beneath our hooves!", () => EncounteredPartyMatch("steppe_bandits") && !HeroIsWounded(), null, 200);
            obj.AddDialogLine("beastmen_die", "start", "close_window", "The dark gods have abandon us!", () => EncounteredPartyMatch("steppe_bandits")&&HeroIsWounded(), null, 200);
            
            obj.AddDialogLine("brokenwheel_greeting", "start", "close_window", "We will break your mind for the glory of Tzeentch", () => EncounteredPartyMatch("chs_cult_1")&& !HeroIsWounded(), null, 200);
            obj.AddDialogLine("brokenwheel_die", "start", "close_window", "the schemes of Tzeentch are endless, you have accomplished nothing!", () => EncounteredPartyMatch("chs_cult_1")&&HeroIsWounded(), null, 200);
            
            obj.AddDialogLine("illumination_greeting", "start", "close_window", "Ascend in death!", () => EncounteredPartyMatch("chs_cult_2")&& !HeroIsWounded(), null, 200);
            obj.AddDialogLine("illumination_die", "start", "close_window", "You may have won the battle, but my life has been more successful than yours will ever be!", () => EncounteredPartyMatch("chs_cult_2")&&HeroIsWounded(), null, 200);
            
            obj.AddDialogLine("secondflesh_greeting", "start", "close_window", "Pox consume you!", () => EncounteredPartyMatch("chs_cult_3")&& !HeroIsWounded(), null, 200);
            obj.AddDialogLine("secondflesh_die", "start", "close_window", "Today, death. Tomorrow, rebirth. The cycle cannot be stopped!", () => EncounteredPartyMatch("chs_cult_3")&&HeroIsWounded(), null, 200);
            
            obj.AddDialogLine("undead_notalk", "start", "close_window", "...", () => CharacterObject.OneToOneConversationCharacter.IsUndead() && CharacterObject.OneToOneConversationCharacter.HeroObject == null, null, 200);
        }

        private bool EncounteredPartyMatch(string clanId)
        {
            if (PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.ActualClan != null)
            {
                return PlayerEncounter.EncounteredMobileParty.ActualClan.StringId == clanId;
            }
            return false;
        }


        private bool HeroIsWounded()
        {
            var hero = CharacterObject.OneToOneConversationCharacter.HeroObject;
            if (hero == null) 
                return false;
            return hero.IsWounded;
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
