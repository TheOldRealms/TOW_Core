using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities;

namespace TOW_Core.CampaignSupport
{
    public class PrisonerFateCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
        }

        private void Initialize(CampaignGameStarter obj)
        {
            _skeleton = MBObjectManager.Instance.GetObject<CharacterObject>("tow_skeleton_recruit");
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.HasWinner)
            {
                foreach (var party in mapEvent.Winner.Parties)
                {
                    if (party.IsNpcParty && (!party.Party.IsMobile || party.Party.MobileParty.IsLordParty))
                    {
                        switch (party.Party.Culture.StringId)
                        {
                            case "empire":
                            case "chaos":
                                KillRegularPrisoners(party);
                                FilterNewMemebers(party, true);
                                break;
                            case "khuzait":
                                SacrificePrisoners(party);
                                FilterNewMemebers(party, false);
                                break;
                        }
                    }
                }
            }
        }

        private void FilterNewMemebers(MapEventParty party, bool shouldKill)
        {
            foreach (var troopRoster in party.RosterToReceiveLootMembers.GetTroopRoster())
            {
                if (troopRoster.Character.Culture != party.Party.Culture)
                {
                    if (!shouldKill)
                    {
                        party.RosterToReceiveLootPrisoners.AddToCounts(troopRoster.Character, troopRoster.Number);
                    }
                    party.RosterToReceiveLootMembers.RemoveTroop(troopRoster.Character, troopRoster.Number);
                }
            }
        }

        private void KillRegularPrisoners(MapEventParty party)
        {
            TOWCommon.Say($"{party.Party} killed {party.RosterToReceiveLootPrisoners.TotalRegulars} prisoners");
            foreach (var troop in party.RosterToReceiveLootPrisoners.GetTroopRoster())
            {
                if (!troop.Character.IsHero)
                {
                    party.RosterToReceiveLootPrisoners.RemoveTroop(troop.Character, troop.Number);
                }
            }
        }

        private void SacrificePrisoners(MapEventParty party)
        {
            var number = Math.Min(party.RosterToReceiveLootPrisoners.TotalRegulars, party.Party.PartySizeLimit - party.Party.MemberRoster.TotalManCount);
            KillRegularPrisoners(party);
            TOWCommon.Say($"{party.Party} raised {number} skeletons");
            if (_skeleton != null)
            {
                party.Party.MemberRoster.AddToCounts(_skeleton, number);
                //party.Party.AddElementToMemberRoster()
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private CharacterObject _skeleton;
    }   
}
