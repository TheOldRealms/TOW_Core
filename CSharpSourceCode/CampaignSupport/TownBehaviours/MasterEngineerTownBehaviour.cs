using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
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
        private bool _gainedTrust;
        private bool _firstMeeting=true;

        private EngineerTrustQuest shopUnlockQuest;

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

        public void FinishedTask()
        {
            
        }

        public bool isMasterEngineerQuestCharacter(Hero character)
        {
            return character == _masterEngineerHero;
        }
        
        
        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            _nuln = Settlement.All.FirstOrDefault(x => x.StringId == "town_WI1");
            
        
            obj.AddDialogLine("engineer_start", "start", "sayopengunshop", "Greetings, Guild Engineer.", engineerstartcondition, null, 200);

           // obj.AddDialogLine("introduction", "engineer_start", "sayopengunshop", "Greetings, I am a master engineer.  how can I help you?", isfirstmeeting, () => _firstMeeting = false, 200);
            
            //first meeting? 
            
            obj.AddPlayerLine("sayopengunshop", "sayopengunshop", "opengunshopCheck", "I need your services", ()=> !questinprogress(), checkplayerrequirements, 200, null);
            
            
            //quest in progress
            obj.AddDialogLine("quest_in_progress", "sayopengunshop", "player_success_test", "Did you finished what I asked you for?",questinprogress, null, 200);
            obj.AddPlayerLine("player_success_test", "player_success_test", "engineer_happy", "yes...",
                engineerquestcompletecondition, handinquest, 200);
            obj.AddPlayerLine("player_on_it", "player_success_test", "engineer_nothappy", "no...",
                questinprogress, null, 200);
            obj.AddDialogLine("engineer_happy", "engineer_happy", "sayopengunshop",
                "Oh these are wonderful news!", null, null, 200);
            obj.AddDialogLine("engineer_nothappy", "engineer_nothappy", "close_window", "Then don't waste my time",questinprogress, null, 200);
            
            
            //player check for entering shop
            obj.AddPlayerLine("sayopengunshop", "engineer_start", "opengunshopCheck", "I need your services.",null, checkplayerrequirements, 200, null);
            obj.AddDialogLine("opengunshopCheck", "opengunshopCheck", "start","Would you give a child a gun? You have not the slightest clue what the technical achievements of the empire are capable of!",
                () => !_playerIsSkilledEnough, null, 200, null);
            obj.AddDialogLine("opengunshopCheck", "opengunshopCheck", "trustCheck", "Oh an engineers colleague, rarely you find a person grasping the concepts of engineering", ()=> _playerIsSkilledEnough, null, 200, null);
            obj.AddDialogLine("trustcheckSucess", "trustCheck", "open_shop", "Come and see what I worked on!",
                playergainedtrust, null, 200);
            
            // quest dialog
            obj.AddDialogLine("trustCheck", "trustCheck", "trustCheck2", "Unfortunately , I can only help a members of the engineers guild or the elector count himself.", () => !playergainedtrust(), null, 200, null);
            obj.AddDialogLine("trustCheck2", "trustCheck2", "playertrustCheck", "However... I could maybe make an exception, if you do me favor...", null, null, 200, null);
            obj.AddPlayerLine("playertrustCheck", "playertrustCheck", "engineerquestexplain", "What do you need help with?",null, null, 200, null);
            obj.AddDialogLine("engineerquestexplain", "engineerquestexplain", "playeracceptquest", "the evil rogue engineer Rudolf took my magnum opus! you have to kill him!",null,null,200,null);
            obj.AddPlayerLine("playeracceptquest", "playeracceptquest", "engineerreactionbeginquest", "I can do that!", null, questbegin, 200,null);
            obj.AddPlayerLine("playeracceptquest", "playeracceptquest", "engineerreactiondeclinequest", "I don't have time for this!", null, null, 200,null);
            obj.AddDialogLine("engineerreactionbeginquest", "engineerreactionbeginquest", "engineer_goodbye",
                "Thank you, I promise it will be worth the trouble!", null, null, 200);
            obj.AddDialogLine("engineerreactiondeclinequest", "engineerreactiondeclinequest", "close_window", "Then don't waste my time", null, null, 200, null);
            
            //open shop
            obj.AddDialogLine("open_shop", "open_shop", "engineer_saygoodbye", "Come and see what I worked on!",
                null, openshopconsequence, 200);
            
            
            //good bye
            obj.AddPlayerLine("engineer_saygoodbye", "engineer_saygoodbye", "engineersaygoodbye", "Farewell Master.", null, null, 200, null);
            obj.AddDialogLine("engineersaygoodbye", "engineersaygoodbye", "close_window", "With fire and steel.", null, null, 200, null);
        }


        private bool questinprogress()
        {
            if (shopUnlockQuest != null)
            {
                return shopUnlockQuest.IsOngoing;
            }
            else return false;
        }
        
        private bool engineerquestcompletecondition()
        {
            if (shopUnlockQuest == null)
                return false;
            if (shopUnlockQuest.JournalEntries[0].HasBeenCompleted())
            {
                return true;
            }


            return false;
        }

        private void handinquest()
        {
            shopUnlockQuest.HandInQuest();
            _gainedTrust = true;
        }
        


        private bool playergainedtrust()
        {
            return _gainedTrust;
        }

        
        private void openshopconsequence()
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

        private void questbegin()
        {
            shopUnlockQuest = EngineerTrustQuest.GetRandomQuest(true);
            shopUnlockQuest?.StartQuest();
            
        }

        private bool isfirstmeeting()
        {
            return _firstMeeting;
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
