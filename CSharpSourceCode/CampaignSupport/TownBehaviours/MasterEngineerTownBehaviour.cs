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
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class MasterEngineerTownBehaviour : CampaignBehaviorBase
    {
        private readonly string _masterEngineerId = "tor_nulnengineernpc_empire";
        private Hero _masterEngineerHero = null;
        private Settlement _nuln;
        private bool _playerIsSkilledEnough;

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
            obj.AddDialogLine("engineer_start", "start", "engineerchoices", "Greetings, Guild Engineer.", engineerstartcondition, null, 200);
            obj.AddPlayerLine("engineer_sayopengunshop", "engineerchoices", "opengunshopCheck", "I need your services.",null, checkplayerrequirements, 200, null);
            obj.AddDialogLine("opengunshopCheck", "opengunshopCheck", "start","Would you give a child a gun? You have not the slightest clue what the technical achievements of the empire are capable of!",
                () => !_playerIsSkilledEnough, null, 200, null);
            obj.AddDialogLine("opengunshopCheck", "opengunshopCheck", "trustCheck", "Oh an engineers colleague, rarely you find a person grasping the concepts of engineering", ()=> _playerIsSkilledEnough, null, 200, null);
            obj.AddDialogLine("trustCheck", "trustCheck", "trustCheck2", "Unfortunately , I can only help a members of the engineers guild or the elector count himself.", null, null, 200, null);
            obj.AddDialogLine("trustCheck2", "trustCheck2", "playertrustCheck", "However... I could maybe make an exception, if you do me favor...", null, null, 200, null);
            obj.AddPlayerLine("playertrustCheck", "playertrustCheck", "engineersaygoodbye", "What do you need help with?",null, questbegin, 200, null);
            obj.AddPlayerLine("engineer_saygoodbye", "engineerchoices", "engineersaygoodbye", "Farewell Master.", null, null, 200, null);
            obj.AddDialogLine("engineer_goodbye", "engineersaygoodbye", "close_window", "With fire and steel.", null, null, 200, null);
        }

        private void openshopconsequence()
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

        private void questbegin()
        {
            var quest = EngineerTrustQuest.GetRandomQuest(true);
            quest?.StartQuest();
        }

        private bool engineerstartcondition()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            if (partner != null && partner.Occupation == Occupation.Special && partner.HeroObject.Name.Contains("Engineer")) return true;
            else return false;
        }

        private void checkplayerrequirements()
        {
            if (Hero.MainHero.GetSkillValue(DefaultSkills.Engineering) >= 50)
                _playerIsSkilledEnough=true;
            else
            {
                _playerIsSkilledEnough = false;
            }

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
