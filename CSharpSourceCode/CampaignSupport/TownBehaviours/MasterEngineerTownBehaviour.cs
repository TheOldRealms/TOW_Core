using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOW_Core.Quests;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class MasterEngineerTownBehaviour : CampaignBehaviorBase
    {
        private readonly string _masterEngineerId = "tor_nulnengineernpc_empire";
        private Hero _masterEngineerHero = null;
        private Settlement _nuln;

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameStarted);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnGameMenuOpened);
            CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener(this, OnBeforeMissionStart);
        }

        private void OnGameMenuOpened(MenuCallbackArgs obj) => EnforceEngineerLocation();
        private void OnBeforeMissionStart() => EnforceEngineerLocation();

        private void EnforceEngineerLocation()
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == _nuln)
            {
                var locationchar = _nuln.LocationComplex.GetLocationCharacterOfHero(_masterEngineerHero);
                var tavern = _nuln.LocationComplex.GetLocationWithId("tavern");
                var currentloc = _nuln.LocationComplex.GetLocationOfCharacter(locationchar);
                if (currentloc != tavern) _nuln.LocationComplex.ChangeLocation(locationchar, currentloc, tavern);
            }
        }

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            _nuln = Settlement.All.FirstOrDefault(x => x.StringId == "town_WI1");
            obj.AddDialogLine("engineer_start", "start", "engineerchoices", "Greetings.", engineerstartcondition, null, 200);
            obj.AddPlayerLine("engineer_sayopengunshop", "engineerchoices", "opengunshop", "Let me see your wares.", null, null, 200, null);
            obj.AddDialogLine("engineer_opengunshop", "opengunshop", "start", "Come have a look.", null, opengunshopconsequence, 200, null);
            obj.AddPlayerLine("engineer_requestquest", "engineerchoices", "requestquest", "Give me the test quest.", requestquestcondition, null, 200, null);
            obj.AddDialogLine("engineer_addquest", "requestquest", "start", "There you go.", null, addquestconsequence, 200, null);
            obj.AddPlayerLine("engineer_saygoodbye", "engineerchoices", "engineersaygoodbye", "Farewell Master.", null, null, 200, null);
            obj.AddDialogLine("engineer_goodbye", "engineersaygoodbye", "close_window", "With fire and steel.", null, null, 200, null);
        }

        private void addquestconsequence()
        {
            var quest = InitialEngineerQuest.GetNew();
            if (quest != null) quest.StartQuest();
        }

        private bool requestquestcondition()
        {
            return InitialEngineerQuest.GetCurrentActiveIfExists() == null;
        }

        private void opengunshopconsequence()
        {
            var engineerItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>().Where(x => x.IsTorItem() && (x.StringId.Contains("gun") || x.StringId.Contains("artillery")));
            List<ItemRosterElement> list = new List<ItemRosterElement>();
            foreach (var item in engineerItems)
            {
                list.Add(new ItemRosterElement(item, MBRandom.RandomInt(1, 5)));
            }
            ItemRoster roster = new ItemRoster();
            roster.Add(list);
            InventoryManager.OpenScreenAsTrade(roster, _nuln.Town);
        }

        private bool engineerstartcondition()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            if (partner != null && partner.Occupation == Occupation.Special && partner.HeroObject.Name.Contains("Engineer")) return true;
            else return false;
        }

        private void OnNewGameStarted(CampaignGameStarter obj)
        {
            foreach(var settlement in Settlement.All)
            {
                if(settlement.StringId == "town_WI1")
                {
                    _nuln = settlement;
                    CreateEngineer();
                }
            }
        }

        private void CreateEngineer()
        {
            CharacterObject template = MBObjectManager.Instance.GetObject<CharacterObject>(_masterEngineerId);
            if (template != null)
            {
                _masterEngineerHero = HeroCreator.CreateSpecialHero(template, _nuln, null, null, 50);
                _masterEngineerHero.SupporterOf = _nuln.OwnerClan;
                _masterEngineerHero.SetName(new TextObject(_masterEngineerHero.FirstName.ToString() + " " + template.Name.ToString()), _masterEngineerHero.FirstName);
                HeroHelper.SpawnHeroForTheFirstTime(_masterEngineerHero, _nuln);
            }
        }

        public override void SyncData(IDataStore dataStore) 
        {
            dataStore.SyncData<Hero>("_masterEngineerHero", ref _masterEngineerHero);
        }
    }
}
