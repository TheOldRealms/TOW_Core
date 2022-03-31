using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Messages.FromLobbyServer.ToClient;
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
        private bool _knowsPlayer;
        private bool _gaveQuestOffer;
        private readonly string _masterEngineerId = "tor_nulnengineernpc_empire";
        private readonly string _rogueEngineerName = "Goswin";
        private Hero _masterEngineerHero = null;
        private Settlement _nuln;
        private bool _playerIsSkilledEnough;
        private bool _playeraskedjoiningTheGuild;
        private bool _gainedTrust;
        private bool _huntedDownCultists;
       
        private RogueEngineerQuest _rogueRogueEngineerQuest;
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

        private void AddEngineerDialogLines(CampaignGameStarter obj)
        {
            obj.AddDialogLine("engineer_start", "start", "questcomplete", "Did you find Oswin?",() => engineerdialogstartcondition() &&knowsplayer()&& CultistQuestIsDone()&&rogueengineerquestinprogress()||quest2failed(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "questcheckrogueengineer", "Have you changed your mind and want to help hunt down Goswin?",() => engineerdialogstartcondition() &&knowsplayer()&& CultistQuestIsDone(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "questcomplete", "Ah, you have returned. What news do you bring?",() => engineerdialogstartcondition() &&knowsplayer()&& cultistquestinprogress()||quest1failed(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "close_window", "Come back to me when you have news.",() => engineerdialogstartcondition() && cultistquestinprogress()&& knowsplayer(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "playergreet", "You again, what do you want?",() => engineerdialogstartcondition()&&knowsplayer()&&!cultistquestinprogress(), null, 200, null);
            obj.AddDialogLine("engineer_start", "start", "playergreet", "You have the look of someone who's never seen a spec of black powder nor grease. Are you in the right place?",engineerdialogstartcondition , knowledgeoverplayer, 200, null);
            obj.AddPlayerLine("playergreet", "playergreet", "playerstartquestcheck", "I have reconsidered your offer, I would like to help.",gavequestoffer , null, 200, null);
            
            //quests failed -both
            obj.AddPlayerLine("questcomplete", "questcomplete", "engineerquestfailed", "I am afraid I have failed to bring what you ask.",() => (quest1failed()|| quest2failed()) , null, 200, null);
            obj.AddDialogLine("engineerquestfailed", "engineerquestfailed", "playerfailedquest", "Tsk, I expected better. There may still be time, you can still track them if you are swift",() => quest1failed()||quest2failed(), null, 200, null);
            obj.AddPlayerLine("playerfailedquest", "playerfailedquest", "engineeracceptquest", "I won't let you down a second time.",quest1failed , QuestBeginCultist, 200, null);
            obj.AddPlayerLine("playerfailedquest", "playerfailedquest", "engineeracceptquest", "I won't let you down a second time.", quest2failed , QuestBeginRogueEngineer, 200, null);
            obj.AddPlayerLine("playerfailedquest", "playerfailedquest", "engineerdeclinequest", "I don't think I can do it at this time.",null , null, 200, null);
            
            //done
            obj.AddPlayerLine("questcomplete", "questcomplete", "cultistengineerdebrief", "I have returned but without the stolen components, I am afraid to say they are still missing.",() => engineerdialogstartcondition() && cultisthuntcompletecondition() , handing_in_cultist_quest, 200, null);
            obj.AddDialogLine("cultistengineerdebrief", "cultistengineerdebrief", "cultistengineerdebrief2", "I see, this is not what I had hoped for. Were there any further clues, did you interrogate these scoundrels? ",null, null, 200, null);
            obj.AddPlayerLine("cultistengineerdebrief2", "cultistengineerdebrief2", "cultistengineerdebrief3", "One of the bandits did mention a name, Goswin I think?",null ,null , 200, null);
            obj.AddDialogLine("cultistengineerdebrief3", "cultistengineerdebrief3", "questrogueengineer", "Blast! I should have known. If you are willing, I would ask for your assistance once more. This matter may be more dire than I originally imagined. Goswin is an Engineer, a good one at that, but his works always seemed...wrong. ",null, null, 200, null);
            obj.AddDialogLine("questrogueengineer", "questrogueengineer", "questcheckrogueengineer", "If he has stolen these parts, it can only be for something heinous. I must ask that you track him down, and put an end to whatever madness he is trying to concoct. Will you do this?",null, null, 200, null);

            // in progress
            obj.AddPlayerLine("questcomplete", "questcomplete", "cultistquestinprogress", "I have yet to track down the runaways.",null , null, 200, null);
            obj.AddDialogLine("cultistquestinprogress", "cultistquestinprogress", "close_window", "I see, return to me when you have something useful.",null, null, 200, null);
            
            //rogue engineer quest

            obj.AddPlayerLine("questcheckrogueengineer", "questcheckrogueengineer", "startrogueengineerquest", "I can, as long as our bargain remains the same. I will find him for you, and in return, you will allow me access to the Forges of Nuln.",null , null, 200, null);
            obj.AddPlayerLine("questcheckrogueengineer", "questcheckrogueengineer", "startrogueengineerquest", "If this is the only way you will allow me access to the forges, then so be it. I will bring you his head.",null , null, 200, null);
            obj.AddPlayerLine("questcheckrogueengineer", "questcheckrogueengineer", "close_window", "I’m afraid not, I have other tasks to attend to.",null , null, 200, null);
            obj.AddDialogLine("startrogueengineerquest", "startrogueengineerquest", "close_window", "We have an agreement then, I believe I may know his whereabouts. I will mark it on your map for you, may Sigmar guide you stranger. ",null, QuestBeginRogueEngineer, 200, null);
            
            //in progress
            obj.AddPlayerLine("rogueengineerquestcomplete", "rogueengineerquestcomplete", "engineerquestinprogress", "I have yet to track him down",null , null, 200, null);
            obj.AddDialogLine("engineerquestinprogress", "engineerquestinprogress", "close_window", "I see, return to me when you have better news.",null, null, 200, null);

            //done
            obj.AddPlayerLine("rogueengineerquestcomplete", "rogueengineerquestcomplete", "engineerquestdebrief", "Oswin will no longer be a problem and I have retrieved what he stole from you. I’m unsure what he was trying to do with them. ",()=> engineerdialogstartcondition()&& engineerquestcompletecondition() , null, 200, null);
            obj.AddDialogLine("engineerquestdebrief", "engineerquestdebrief", "hubaftermission", "It matters not, it would have been something warped no doubt. I must thank you for your efforts, and your discretion. As agreed upon, you may now access our foundries and purchase from us as you please. ", null, handing_in_rogueengineer_quest, 200, null);
            obj.AddDialogLine("hubaftermission", "hubaftermission", "hub", "Now, how can I help?", null, null, 200);
            
            //skill check
            obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "Greetings Master Engineer, I am "+Campaign.Current.MainParty.LeaderHero.Name.ToString()+". I have come seeking access to the Forges of Nuln. Can you help?.",null , null, 200, null);
            obj.AddPlayerLine("playergreet", "playergreet", "opengunshopcheck", "I am, I have come seeking access to black powder weapons.",null , null, 200, null);
            obj.AddDialogLine("opengunshopcheck", "opengunshopcheck", "skillcheck","Hah!, you don’t look like you would even know what to do with them. What could you possibly need with our crafts?", null, checkplayerengineerskillrequirements, 200, null);
            obj.AddDialogLine("playerskillcheckfailed", "skillcheck", "close window", "I am far too busy for this, leave my sight.", ()=> !playerisSkilledEnough(), null, 200);
            obj.AddDialogLine("playerskillchecksuccess", "skillcheck", "playerpassedskillcheck2", "These are the mightiest weapons of the Empire, they hold back the tide of darkness time and again. We do not hand out our crafts to any who waltz in, I do not know you. Nor have you earned our trust.", playerisSkilledEnough, null, 200);
            //quest
            
            obj.AddDialogLine("playerpassskillcheck2", "playerpassedskillcheck2", "playerstartquestcheck", "We may however be able to come to an agreement, there is an internal matter that needs urgent attention and I am unable to act. If you help, as a personal favour, I will see what I can do for you. What say you?", null, givequestoffer, 200);
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "explainquest", "What would you have me do?",null, null, 200, null);
            obj.AddPlayerLine("playerstartquestcheck", "playerstartquestcheck", "engineerdeclinequest", " I don't have time for this.", null, null, 200,null);
            obj.AddDialogLine("explainquest", "explainquest", "questcheck", "Usually we don’t resort to outside assistance but we are short handed, we have had some important components stolen from the Colleges of Nuln by "+ _rogueEngineerName+" and they must be returned. Immediately. If you can track down these runaways and find these parts then we can talk further.", null,null,200);

            //accept decline quest
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "I understand, I will return the moment I have news.",null, QuestBeginCultist, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineeracceptquest", "That is all it will take? Sounds easy enough.",null, QuestBeginCultist, 200, null);
            obj.AddPlayerLine("questcheck", "questcheck", "engineerdeclinequest", "I do not have time for this.", null, null, 200,null);
            obj.AddDialogLine("engineeracceptquest", "engineeracceptquest", "close_window", "Good, I expect positive results and your hasty return.", null, null, 200);
            obj.AddDialogLine("engineerdeclinequest", "engineerdeclinequest", "close_window", "A shame, think on it and return if you change your mind.", null, null, 200, null);
            
            //hub player
            obj.AddPlayerLine("hub", "hub", "opengunshop", "I would like to buy some cannons.",null , null, 200, null);
            obj.AddPlayerLine("hub", "hub", "recruitengineer", "I would like to recruit some engineers.",null , null, 200, null);
            obj.AddPlayerLine("hub", "hub", "tutorialcannonbuy", "How do I buy more cannons?",null , null, 200, null);
            obj.AddPlayerLine("hub", "hub", "opengunshop", "How can I use cannons?",null , null, 200, null);
            obj.AddPlayerLine("hub", "hub", "close_window", "Nothing at the moment, I must leave.",null , null, 200, null);

            // shop
            obj.AddDialogLine("opengunshop", "opengunshop", "opengunshopandclosedialog", "Of course, you'll find only the best from the Forges of Nuln!", null, null, 200);
            obj.AddDialogLine("opengunshopandclosedialog", "opengunshopandclosedialog", "hub", "What else can I do for you?", null, opengunshopconsequence, 200);
           //recruitment
            obj.AddDialogLine("recruitengineer", "recruitengineer", "openrecruitmentandclosedialog", "Of course, you'll find only the best from the Forges of Nuln!", null, null, 200);
            obj.AddDialogLine("openrecruitmentandclosedialog", "openrecruitmentandclosedialog", "hub", "What else can I do for you?", null, cannoncrewrecruitmentconsequence, 200);
            //tutorial buy cannons
            obj.AddDialogLine("tutorialcannonbuy", "tutorialcannonbuy", "tutorialcannonbuy2", "To buy cannons you must be in the service of an Imperial Elector Count", null, null, 200);
            obj.AddDialogLine("tutorialcannonbuy2", "tutorialcannonbuy2", "tutorialcannonbuy3", "The amount of cannons you can field in your army increases every 50 levels in Engineering skill.", null, null, 200);
            obj.AddDialogLine("tutorialcannonbuy3", "tutorialcannonbuy3", "hub", "If you have met these requirements, simply speak to me and I'll show you what we have.", null, null, 200);
            //tutorial use cannons
            obj.AddDialogLine("tutorialcannonuse", "tutorialcannonuse", "tutorialcannonuse2", "Cannons are placed using Q, but to fire the cannons you will need to hire at least two Cannon Crew", null, null, 200);
            obj.AddDialogLine("tutorialcannonuse2", "tutorialcannonuse2", "tutorialcannonuse3", "You will also need to ensure that the cannon is in your party inventory", null, null, 200);
            obj.AddDialogLine("tutorialcannonuse3", "tutorialcannonuse3", "hub", "Engineers and Cannon Crew can both fire cannons.", null, null, 200);
            
        }

        private void AddCultistDialogLines(CampaignGameStarter obj)
        {
            obj.AddDialogLine("engineerquestcultist_start", "start", "cultist_answerplayer", "Goswin was right, they sent someone after us! Grab your weapons quickly!",cultiststartcondition, null, 200);
            obj.AddPlayerLine("cultist_answerplayer", "cultist_answerplayer", "cultist_answer", "Woah hold there, I have merely come for the stolen parts, there is no need to shed blood here. Perhaps an arrangement can be made?", null, null, 200);
            obj.AddPlayerLine("cultist_answerplayer", "cultist_answerplayer", "cultist_answer", "Lay down your weapons and I may spare your lives.", null, null, 200);
            obj.AddPlayerLine("cultist_answerplayer", "cultist_answerplayer", "close_window", "Weapons or no, we will slay you all and take back what you stole!", null, null, 200);
            obj.AddDialogLine("cultist_answer", "cultist_answer", "close_window", "You will not trick us!  They will serve a greater purpose! You will not take them!", null, null, 200);
        }
        
        private void AddRogueEngineerDialogLines(CampaignGameStarter obj)
        {
            obj.AddDialogLine("rogueengineer_start", "start", "rogueengineer_answerplayer", "So the old fool sent you to find me? How did he figure me out? It matters not, you will stand in the way of my creations. You will die here!",rogueengineerdialogstartcondition, null, 200);
            
            //requires dying dialog of the engineer
            obj.AddPlayerLine("rogueengineer_playerafterbattle", "rogueengineer_playerafterbattle", "close_window", "Your schemes end here",null, null, 200);

        }
        
        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            _nuln = Settlement.All.FirstOrDefault(x => x.StringId == "town_WI1");
            
            AddEngineerDialogLines(obj);
            
            AddCultistDialogLines(obj);
            
            AddRogueEngineerDialogLines(obj);
        }
        
        private bool rogueengineerquestinprogress()
        {
            return _rogueRogueEngineerQuest != null && _rogueRogueEngineerQuest.IsOngoing;
        }

        private bool cultistquestinprogress()
        {
            return _cultistKillQuest != null && _cultistKillQuest.IsOngoing;
        }
        
        
        private bool quest1failed()
        {
            if (_cultistKillQuest == null) return false;
            return _cultistKillQuest.GetQuestFailed();
        }
        
        private bool quest2failed()
        {
            if (_rogueRogueEngineerQuest == null) return false;
            return _rogueRogueEngineerQuest.FailState;
        }
        
        private bool engineerquestcompletecondition()
        {
            if (_rogueRogueEngineerQuest == null)
                return false;
            if (_rogueRogueEngineerQuest.JournalEntries[0].HasBeenCompleted())
            {
                return true;
            }
            
            return false;
        }
        
        private bool cultisthuntcompletecondition()
        {
            if (_cultistKillQuest == null)
                return false;
            if (_cultistKillQuest.JournalEntries[0].HasBeenCompleted())
            {
                return true;
            }
            
            return false;
        }
        
        private void handing_in_rogueengineer_quest()
        {
            _rogueRogueEngineerQuest.HandInQuest();
            var xp =(float)  _rogueRogueEngineerQuest.RewardXP;
            SkillObject skill = DefaultSkills.Engineering;
            Campaign.Current.MainParty.LeaderHero.AddSkillXp(skill,xp);
            _gainedTrust=true;
        }
        
        private void handing_in_cultist_quest()
        {
            _cultistKillQuest.HandInQuest();
            var xp =(float) _cultistKillQuest.GetRewardXP();
            SkillObject skill = DefaultSkills.Charm;
            Campaign.Current.MainParty.LeaderHero.AddSkillXp(skill,xp);
            _huntedDownCultists = true;
        }
        
        private bool CultistQuestIsDone()
        {
            if (_cultistKillQuest == null) return false;
            if (_cultistKillQuest.GetQuestFailed()) return false;
            return _cultistKillQuest.IsFinalized;
        }

        private void givequestoffer()
        {
            _gaveQuestOffer = true;
        }

        private bool gavequestoffer()
        {
            return _gaveQuestOffer;
        }
        
        private bool playergainedtrust()
        {
            return engineerdialogstartcondition() && _gainedTrust;
        }

        
        private void openshopconsequence()
        {
            var quest = RogueEngineerQuest.GetNew();
            if (quest != null) quest.StartQuest();
        }

        private bool requestquestcondition()
        {
            return RogueEngineerQuest.GetCurrentActiveIfExists() == null;
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
        
        private void cannoncrewrecruitmentconsequence()
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

        private void QuestBeginCultist()
        {
            if (_cultistKillQuest != null)
            {
                _cultistKillQuest = null;
            }
                
            
            _cultistKillQuest = CultistQuest.GetNew("Part Thieves");
            _cultistKillQuest?.StartQuest();
        }
        
        private void QuestBeginRogueEngineer()
        {
            _rogueRogueEngineerQuest = RogueEngineerQuest.GetNew();
            _rogueRogueEngineerQuest?.StartQuest();
        }
        

        private void knowledgeoverplayer()
        {
            _knowsPlayer = true;
        }

        private bool knowsplayer()
        {
            return _knowsPlayer;
        }

        private bool engineerdialogstartcondition()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            if (partner != null && partner.Occupation == Occupation.Special && partner.HeroObject.Name.Contains("Engineer")) return true;
            else return false;
        }

        private bool cultiststartcondition()
        {
            if (_cultistKillQuest == null) return false;
            
            if (!_cultistKillQuest.IsOngoing) return false;


            if (Campaign.Current.ConversationManager.ConversationParty == _cultistKillQuest.GetTargetParty())
                return true;
            
            return false;
            //return partner. == _cultistKillQuest.GetTargetParty();
        }

        private bool rogueengineerdialogstartcondition()
        {
            if (_rogueRogueEngineerQuest==null) return false;
            
             if(!_rogueRogueEngineerQuest.IsOngoing) return false;

            if (Campaign.Current.CurrentConversationContext != ConversationContext.PartyEncounter) return false;
            
            var partner = CharacterObject.OneToOneConversationCharacter;
            return partner != null && partner.Occupation == Occupation.Lord &&
                   partner.HeroObject.Name.Contains(_rogueRogueEngineerQuest.RogueEngineerName);
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
            dataStore.SyncData<bool>("_gaveQuestOffer", ref _gaveQuestOffer);
            dataStore.SyncData<bool>("_knowsPlayer", ref _knowsPlayer);
            dataStore.SyncData<Hero>("_masterEngineerHero", ref _masterEngineerHero);
            dataStore.SyncData<CultistQuest>("_cultistKillQuest", ref _cultistKillQuest);
            dataStore.SyncData<RogueEngineerQuest>("_rogueEngineerQuest", ref _rogueRogueEngineerQuest);
        }
    }
}
