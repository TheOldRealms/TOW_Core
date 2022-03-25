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
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Quests;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    public class MasterEngineerTownBehaviour : CampaignBehaviorBase
    {
        private readonly string _masterEngineerId = "tor_nulnengineernpc_empire";
        private readonly string _rogueEngineerName = "Rudolf";
        private Hero _masterEngineerHero = null;
        private Settlement _nuln;
        private bool _playerIsSkilledEnough;
        private bool _playeraskedjoiningTheGuild;
        private bool _gainedTrust;
        private bool _firstMeeting=true;

        //private EngineerTrustQuest _cultistKillQuest;
        
        
        private  CultistQuest _cultistKillQuest;

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
            
            //greeting dialog
            obj.AddDialogLine("engineer_start", "start", "close_window", "Come back to me when you have news.",() => engineerstartcondition() && questchaininprogress()&& issecondmeeting(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "close_window", "You again, what do you want?",() => engineerstartcondition() && !questchaininprogress()&& issecondmeeting(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "playergreet", "You again, what do you want?",() => engineerstartcondition()&&issecondmeeting()&&!questchaininprogress(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "playergreet", "You have the look of someone who's never seen a spec of black powder nor grease. Are you in the right place?",engineerstartcondition , knowledgeoverplayer, 200, null);
            
            //quest turn in
            
            //failed
            obj.AddPlayerLine("playergreet", "start", "explainquest", "I have reconsidered your offer, I would like to help.",()=> engineerstartcondition()&& quest1failed() , null, 200, null);
            obj.AddPlayerLine("playergreet", "start", "close_window", "Nothing, I must take my leave.",()=> engineerstartcondition()&& quest1failed() , null, 200, null);
            
            
            
            //skill check
            obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "Greetings Master Engineer, I am "+Campaign.Current.MainParty.LeaderHero.Name.ToString()+". I have come seeking access to the Forges of Nuln. Can you help?.",null , null, 200, null);
            obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "I am, I have come seeking access to black powder weapons.",null , null, 200, null);
            obj.AddDialogLine("opengunshopcheck", "opengunshopcheck", "skillcheck","Hah!, you don’t look like you would even know what to do with them. What could you possibly need with our crafts?", null, checkplayerengineerskillrequirements, 200, null);
            obj.AddDialogLine("playerskillcheckfailed", "skillcheck", "close window", "I am far too busy for this, leave my sight.", ()=> !playerisSkilledEnough(), null, 200);
            obj.AddDialogLine("playerskillchecksuccess", "skillcheck", "playerpassedskillcheck2", "These are the mightiest weapons of the Empire, they hold back the tide of darkness time and again. We do not hand out our crafts to any who waltz in, I do not know you. Nor have you earned our trust.", playerisSkilledEnough, null, 200);
            
            
            //quest
            obj.AddDialogLine("playerpassskillcheck2", "playerpassedskillcheck2", "playerstartquestcheck", "We may however be able to come to an agreement, there is an internal matter that needs urgent attention and I am unable to act. If you help, as a personal favour, I will see what I can do for you. What say you?", null, null, 200);
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "explainquest", "What would you have me do?",null, null, 200, null);
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "engineerdeclinequest", " I don't have time for this.", null, null, 200,null);
            obj.AddDialogLine("explainquest", "explainquest", "questcheck", "Usually we don’t resort to outside assistance but we are short handed, we have had some important components stolen from the Colleges of Nuln by "+ _rogueEngineerName+" and they must be returned. Immediately. If you can track down these runaways and find these parts then we can talk further.", null,null,200);

            //accept decline quest
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "I understand, I will return the moment I have news.",null, questbegin, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "That is all it will take? Sounds easy enough.",null, questbegin, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineerdeclinequest", "I do not have time for this.", null, null, 200,null);
            obj.AddDialogLine("engineeracceptquest", "engineeracceptquest", "close_window", "Good, I expect positive results and your hasty return.", null, null, 200);
            obj.AddDialogLine("engineerdeclinequest", "engineerdeclinequest", "close_window", "A shame, think on it and return if you change your mind.", null, null, 200, null);
            
            
            
            
            
            
            
               
            /*obj.AddDialogLine("quest_in_progress", "start", "player_success_test", "Did you finished what I asked you for?",questchaininprogress, null, 200, null);
            
            obj.AddDialogLine("engineer_start", "start", "huboffers", "Hello Friend.",playergainedtrust , null, 210, null);
            
        
            
            
           // obj.AddDialogLine("introduction", "engineer_start", "sayopengunshop", "Greetings, I am a master engineer.  how can I help you?", isfirstmeeting, () => _firstMeeting = false, 200);
           //first meeting? 
            //obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "Greetings Master Engineer, I am "+Campaign.Current.MainParty.LeaderHero.Name.ToString()+". I have great interest in becoming an Engineer and have some questions about your order.",null , null, 200, null);
            //obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "I am, I have come seeking access to black powder weapons.",null , null, 200, null);
            
            //quest in progress
            
            obj.AddPlayerLine("player_success_test", "player_success_test", "engineer_happy", "yes...",
                engineerquestcompletecondition, handinquest, 200);
            obj.AddPlayerLine("player_on_it", "player_success_test", "engineer_nothappy", "no...",
                questchaininprogress, null, 200);
            obj.AddDialogLine("engineer_happy", "engineer_happy", "hubOffers",
                "Oh these are wonderful news!", null, null, 200);
            obj.AddDialogLine("engineer_nothappy", "engineer_nothappy", "close_window", "Then don't waste my time",questchaininprogress, null, 200);
            
            
            //player check for entering shop
            //obj.AddPlayerLine("sayopengunshop", "engineer_start", "opengunshopCheck", "I need your services.",null, checkplayerengineerskillrequirements, 200, null);
            
            obj.AddDialogLine("opengunshopcheck", "opengunshopcheck", "playerexplain","Hah!, you don’t look like you would even know what to do with them. What could you possibly need with our crafts?", null, null, 200, null);
            obj.AddDialogLine("hubOffers", "hubOffers", "playerexplain","What can I do for you?", null, null, 200, null);
            obj.AddPlayerLine("playerexplain", "playerexplain", "skillcheckjoin", "Be that as it may, I wish to become an engineer and further my knowledge!", ()=> !_gainedTrust, checkplayerengineerskillrequirements, 200,null);
            obj.AddPlayerLine("playerexplain", "playerexplain", "skillcheckshop", "It is well known that weapons crafted by your order are powerful, I have come for these weapons!", ()=> !_gainedTrust, checkplayerengineerskillrequirements, 200,null);
            
            obj.AddPlayerLine("playerexplain", "playerexplain", "open_shop", "Let me see your goods", playergainedtrust, null, 200,null);
            obj.AddPlayerLine("playerexplain", "playerexplain", "close_window", "I am just looking", playergainedtrust, null, 200,null);
            obj.AddPlayerLine("playerexplain", "playeracceptquest", "close_window", "Perhaps you are right. I must leave.", null, null, 200,null);
            
            
            // player can't join the engineers, however these line serve placeholders and change maybe later in the project
            obj.AddDialogLine("skillcheckjoin", "skillcheckjoin", "hubOffers", "You will have to find another way to learn.", ()=> !_playerIsSkilledEnough, null, 200, null);
            obj.AddDialogLine("skillcheckjoin", "skillcheckjoin", "hubOffers", "We are currently not accepting any new applicants.", ()=> _playerIsSkilledEnough, null, 200, null);
            
            //open gun shop checks before quests
            obj.AddDialogLine("skillcheckshop", "skillcheckshop", "playerpassedskillcheck", "These are the mighty weapons of the Empire, the very creations that hold back the tide of darkness time and again. We do hand out our crafts to any who waltz in, I do not know you. Nor have you earned our trust!",
                null, checkplayerengineerskillrequirements, 200);
            
            obj.AddDialogLine("playerpassedskillcheck", "playerpassedskillcheck", "trustdialog", "You look like someone who who won't blast their head off using gun powder. Which doesn't change that I don't trust you",
                ()=> _playerIsSkilledEnough, null, 200);
            obj.AddDialogLine("playerpassedskillcheck", "playerpassedskillcheck", "close window", "You barely understand the implications of using basic physics! Do not waste my time",
                ()=> _playerIsSkilledEnough, null, 200);
            
            obj.AddDialogLine("trustdialog", "trustdialog", "playerstartquestcheck", " We may however be able to come to some form of agreement, there is an internal matter that needs urgent attention and I am unable to act. If you want access to our arsenal then you must help us first.",
                 null, null,200);
            
            //answer if quests are okay
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "startquest", "What is the task?",null, null, 200, null);
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "engineerdeclinequest", "I don't have time for this!", null, null, 200,null);
            
            //explain quest
            obj.AddDialogLine("startquest", "startquest", "questcheck", "Usually we don’t resort to outside assistance but we are short handed, we have had some important components stolen from the Colleges of Nuln by "+ _rogueEngineerName+" and they must be returned. Immediately. If you can track down these runaways and find these parts then we can talk further.",
                null,null,200);
            
            //accept deline quest
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "It will be done, I will return the moment I have news.",null, questbegin, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "That is all it will take? Sounds easy enough.",null, questbegin, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineerdeclinequest", "I don't have time for this!", null, null, 200,null);
            
            
            //reaction engineer
            obj.AddDialogLine("engineeracceptquest", "engineeracceptquest", "close_window",
                "Good, I expect positive results and your hasty return.", null, null, 200);
            obj.AddDialogLine("engineerdeclinequest", "engineerdeclinequest", "close_window", "A shame, think on it and return if you change your mind.", null, null, 200, null);
            
            
            //questrelated
            obj.AddDialogLine("rogueengineer_greet", "start", "playermetrogueengineer", "hahaha i am evil",
                rogueengineerstartcondition, null, 200);
            obj.AddPlayerLine("rogueengineer_greet", "playermetrogueengineer", "close_window", "hahaha i am more evil",
                null, null, 200);
                */
            
            
            
          //  obj.AddDialogLine("open_shop", "open_shop", "engineersaygoodbye", "Come and see what I worked on!", null,null, 200);
       //     obj.AddDialogLine("engineersaygoodbye", "engineersaygoodbye", "close_window", "With fire and steel.", null, opengunshopconsequence, 200, null);*/
           
            /*// quest dialog
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
            
               
            
            
            //good bye
            obj.AddPlayerLine("engineer_saygoodbye", "engineer_saygoodbye", "engineersaygoodbye", "Farewell Master.", null, null, 200, null);
            obj.AddDialogLine("engineersaygoodbye", "engineersaygoodbye", "close_window", "With fire and steel.", null, null, 200, null);*/
        }

        
        

        private bool questchaininprogress()
        {
            if (engineerstartcondition())
                return _cultistKillQuest != null && _cultistKillQuest.IsOngoing;
            else return false;
        }
        
        
        private bool quest1failed()
        {
            return _cultistKillQuest.GetQuestFailed();
        }
        
        private bool engineerquestcompletecondition()
        {
            if (_cultistKillQuest == null)
                return false;
            if (_cultistKillQuest.JournalEntries[0].HasBeenCompleted())
            {
                return true;
            }
            
            return false;
        }

        private void handinquest()
        {
            _cultistKillQuest.HandInQuest();
            _gainedTrust = true;
        }
        

        private bool playergainedtrust()
        {
            return engineerstartcondition() && _gainedTrust;
        }

        
        private void openshopconsequence()
        {
            var quest = EngineerTrustQuest.GetNew();
            if (quest != null) quest.StartQuest();
        }

        private bool requestquestcondition()
        {
            return EngineerTrustQuest.GetCurrentActiveIfExists() == null;
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
            _cultistKillQuest = CultistQuest.GetNew("Part Thieves");
            _cultistKillQuest?.StartQuest();
            
        }

        private void knowledgeoverplayer()
        {
            _firstMeeting = false;
        }

        private bool issecondmeeting()
        {
            return !_firstMeeting;
        }

        private bool engineerstartcondition()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            if (partner != null && partner.Occupation == Occupation.Special && partner.HeroObject.Name.Contains("Engineer")) return true;
            else return false;
        }

        private bool rogueengineerstartcondition()
        {
            if (!_cultistKillQuest.IsOngoing) return false;

            if (Campaign.Current.CurrentConversationContext != ConversationContext.PartyEncounter) return false;
            
            var partner = CharacterObject.OneToOneConversationCharacter;
            /*return partner != null && partner.Occupation == Occupation.Lord &&
                   partner.HeroObject.Name.Contains(_cultistKillQuest.GetRogueEngineerName());*/
            return false;
        }

        private void checkplayerengineerskillrequirements()
        {
            if (Hero.MainHero.GetSkillValue(DefaultSkills.Engineering) >= 50)
                _playerIsSkilledEnough=true;
            else
            {
                _playerIsSkilledEnough = false;
            }
        }


        private bool playerisSkilledEnough()
        {
            return _playerIsSkilledEnough;
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
            
            dataStore.SyncData<CultistQuest>("_cultistKillQuest", ref _cultistKillQuest);
        }
    }
}
