using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    public class TORPartyHealCampaignBehavior : PartyHealCampaignBehavior
    {
        public override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(HealParty));
        }

        private void HealParty(MobileParty party)
        {
            if (party.IsActive && party.MapEvent == null)
            {
                foreach (var troopRoster in party.MemberRoster.GetTroopRoster())
                {
                    if (troopRoster.Character.IsHero && troopRoster.Character.HeroObject.IsVampire())
                    {
                        troopRoster.Character.HeroObject.Heal(party.Party, 20, false);
                    }
                }
            }
        }
    }
}
