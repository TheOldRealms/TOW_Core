using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    class TavernBooksSellerTownBehaviour : CampaignBehaviorBase
    {
        // TODO: Replace with culture friendly template?
        private static readonly string _scrollSellerId = "tor_scolltrader";

        private CharacterObject _scrollSellerObject;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
        }

        public override void SyncData(IDataStore dataStore) {}

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            AddScrollSellerDialogue(obj);
        }

        // TODO: Replace with more lore friendly dialogue
        private void AddScrollSellerDialogue(CampaignGameStarter obj)
        {
            obj.AddDialogLine("scroll_trader_greet", "start", "scroll_trade", "Do you want to buy some scrolls?", () => IsScrollSeller(), null, 200, null);
            obj.AddPlayerLine("scroll_trader_greet_yes_response", "scroll_trade", "end_scroll_trade", "Yes.", null, null, 200, null);
            obj.AddPlayerLine("scroll_trader_greet_no_response", "scroll_trade", "close_window", "No.", null, null, 200, null);

            obj.AddDialogLine("end_scroll_trade", "end_scroll_trade", "end_scroll_trade", "Thanks for shopping!", null, OpenScrollShop, 200, null);
            obj.AddPlayerLine("end_scroll_trade_reopen_response", "end_scroll_trade", "end_scroll_trade", "Can I look at your shop again?", null, null, 200, null);
            obj.AddPlayerLine("end_scroll_trade_bye_response", "end_scroll_trade", "close_window", "Bye!", null, null, 200, null);
        }

        private void OpenScrollShop()
        {
            // TODO: Replace with actual books / scroll assets.
            var scrollItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>().Where(x => x.StringId.Contains("ironIngot"));
            List<ItemRosterElement> list = new List<ItemRosterElement>();
            foreach (var item in scrollItems)
            {
                list.Add(new ItemRosterElement(item, MBRandom.RandomInt(1, 5)));
            }
            ItemRoster roster = new ItemRoster();
            roster.Add(list);
            InventoryManager.OpenScreenAsTrade(roster, Settlement.CurrentSettlement.Town);
        }

        private bool IsScrollSeller()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            return partner != null
                && partner.Occupation == Occupation.Merchant
                && partner.StringId.Equals(_scrollSellerId);
        }

        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            if (settlement.IsTown && CampaignMission.Current != null)
            {
                Location location = CampaignMission.Current.Location;
                if (location != null && location.StringId == "tavern")
                {
                    location.AddLocationCharacters(new CreateLocationCharacterDelegate
                        (CreateBooksAndScrollsSeller),
                        settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
                }
            }
        }

        private LocationCharacter CreateBooksAndScrollsSeller(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            _scrollSellerObject = MBObjectManager.Instance.GetObject<CharacterObject>(_scrollSellerId);
            if (_scrollSellerObject == null)
            {
                return null;
            }

            var locationCharacter = new LocationCharacter(new AgentData(
                new SimpleAgentOrigin(
                    _scrollSellerObject, -1, null, default(UniqueTroopDescriptor))).Monster(Campaign.Current.HumanMonsterSettlement),
                    new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                    "npc_common", true, relation, null, false, false, null, false, false, true);

            return locationCharacter;
        }
    }
}
