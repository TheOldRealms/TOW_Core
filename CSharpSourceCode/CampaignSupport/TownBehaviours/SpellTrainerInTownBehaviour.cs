using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.Abilities.SpellBook;
using TOW_Core.Quests;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport.TownBehaviours
{
    class SpellTrainerInTownBehaviour : CampaignBehaviorBase
    {
        private readonly string _empireTrainerId = "tor_spelltrainer_empire_0";
        private readonly string _vampireTrainerId = "tor_spelltrainer_vc_0";
        private string _testResult = "";

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        private void OnNewGameCreated(CampaignGameStarter obj)
        {
            foreach (var settlement in Settlement.All)
            {
                if (settlement.IsTown)
                {
                    CreateTrainer(settlement);
                }
            }
        }

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            AddDialogs(obj);
        }

        private void AddDialogs(CampaignGameStarter obj)
        {
            obj.AddDialogLine("trainer_start", "start", "choices", "Do I know you? What do you need, be quick I am a busy.", spelltrainerstartcondition, null, 200, null);
            obj.AddPlayerLine("trainer_test", "choices", "magictest", "{TEST_QUESTION}", magictestcondition, null, 200, null);
            obj.AddDialogLine("trainer_testoutcome", "magictest", "testoutcome", "{TEST_PROMPT}", filltextfortestprompt, determinetestoutcome, 200, null);
            obj.AddDialogLine("trainer_testresult", "testoutcome", "start", "{TEST_RESULT}", testresultcondition, null, 200, null);
            obj.AddPlayerLine("trainer_learnspells", "choices", "openbook", "I have come seeking further knowledge.", () => Hero.MainHero.IsSpellCaster(), null, 200, null);
            obj.AddDialogLine("trainer_afterlearnspells", "openbook", "start", "Hmm, come then. I will teach you what I can.", null, openbookconsequence, 200, null);
            obj.AddPlayerLine("trainer_howtoadvance", "choices", "getquest", "I wish to grow stronger and harness even more power, how can I do this? ", () => AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists() == null && Hero.MainHero.GetExtendedInfo().SpellCastingLevel < SpellCastingLevel.Master && Hero.MainHero.GetExtendedInfo().SpellCastingLevel > SpellCastingLevel.None, null, 200, null);
            obj.AddDialogLine("trainer_getadvancequest", "getquest", "start", "You will need to test your strength, use what you have learned already and demonstrate your abilities. ", null, advancequestconsequence, 200, null);
            obj.AddPlayerLine("trainer_specialize", "choices", "specializelore", "{SPECALIZE_QUESTION}", specializelorecondition, null, 200, null);
            obj.AddDialogLine("trainer_chooselore", "specializelore", "start", "{SPECIALIZE_PROMPT}.", fillchooseloretext, chooseloreconsequence, 200, null);
            obj.AddPlayerLine("trainer_increaselevel", "choices", "increasecasterlevel", "{INCREASE_LEVEL}", increasecasterlevelcondition, null, 200, null);
            obj.AddDialogLine("trainer_confirmnewlevel", "increasecasterlevel", "start", "{INCREASE_RESULT}", fillincreaseoptiontext, increasecasterlevelconsequence, 200, null);
            obj.AddPlayerLine("trainer_playergoodbye", "choices", "saygoodbye", "Farewell Magister.", null, null, 200, null);
            obj.AddDialogLine("trainer_goodbye", "saygoodbye", "close_window", "Hmm, yes. Farewell.", null, null, 200, null);
        }

        private bool fillincreaseoptiontext()
        {
            string text = "";
            var culture = Hero.OneToOneConversationHero.Culture.StringId;
            switch (culture)
            {
                case "empire":
                    {
                        text = "Your power is without question and your skill evident; you are a welcome addition to the Orders magisters and may access more knowledge from our libraries and tutors.";
                        break;
                    }
                case "khuzait":
                    {
                        text = "Your affinity for dark magic has grown greatly since we last spoke. Yes you will serve me well, let us continue your training. Today I will show you something special, it just arrived from Nehekhara...";
                        if (Hero.MainHero.IsVampire()) text = "Oh how strong you have become my sire! You may perhaps rival Manfred himself one day, and should you please do remember your humble tutor.";
                        break;
                    }
                default:
                    text = "You shouldn't see this.";
                    break;
            }
            MBTextManager.SetTextVariable("INCREASE_RESULT", text);
            return true;
        }

        private bool fillchooseloretext()
        {
            string text = "";
            var culture = Hero.OneToOneConversationHero.Culture.StringId;
            switch (culture)
            {
                case "empire":
                    {
                        text = "You have proven yourself and have a strong aptitude for the wind of magic, which college will you be joining? You can only dedicate yourself to one, choose wisely.";
                        break;
                    }
                case "khuzait":
                    {
                        text = "You have potential Dark One, serve me and I will teach you all that I know in time. Come let me consult my grimoire.";
                        if (Hero.MainHero.IsVampire()) text = "Yes sire! Right away my good and merciful lord.";
                        break;
                    }
                default:
                    text = "You shouldn't see this.";
                    break;
            }
            MBTextManager.SetTextVariable("SPECIALIZE_PROMPT", text);
            return true;
        }

        private bool filltextfortestprompt()
        {
            string text = "";
            var culture = Hero.OneToOneConversationHero.Culture.StringId;
            switch (culture)
            {
                case "empire":
                    {
                        text = "Hmm. To understand the Winds of magic you must have the aethyric senses. Let me perform an experiment on you to determine your potential... (30% Chance)";
                        break;
                    }
                case "khuzait":
                    {
                        text = "I can sense the you might have some grasp on the Winds of magic. Let me subject you to an examination to see your potential...  (30% Chance)";
                        break;
                    }
                default:
                    text = "You shouldn't see this.";
                    break;
            }
            MBTextManager.SetTextVariable("TEST_PROMPT", text);
            return true;
        }

        private bool magictestcondition()
        {
            var flag = false;
            flag = !Hero.MainHero.IsVampire() && !Hero.MainHero.IsSpellCaster() && _testResult == "";
            if (flag)
            {
                string text = "";
                var culture = Hero.OneToOneConversationHero.Culture.StringId;
                switch (culture)
                {
                    case "empire":
                        {
                            text = "I have come seeking knowledge, I wish to learn the arcane arts. Can you help me?";
                            break;
                        }
                    case "khuzait":
                        {
                            text = "I need the power to escape death, to rule over this world as something more. Can you teach me the ways of your power?";
                            break;
                        }
                    default:
                        text = "You shouldn't see this.";
                        break;
                }
                MBTextManager.SetTextVariable("TEST_QUESTION", text);

            }
            return flag;
        }

        private void advancequestconsequence()
        {
            var quest = AdvanceSpellCastingLevelQuest.GetRandomQuest(true);
            if (quest != null) quest.StartQuest();
        }

        private void increasecasterlevelconsequence()
        {
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            if (quest != null) quest.CompleteQuestWithSuccess();
            var info = Hero.MainHero.GetExtendedInfo();
            if (info != null)
            {
                Hero.MainHero.SetSpellCastingLevel((SpellCastingLevel)Math.Min((int)info.SpellCastingLevel + 1, 4));
                if((int)info.SpellCastingLevel < (int)SpellCastingLevel.Master)
                {
                    quest = AdvanceSpellCastingLevelQuest.GetRandomQuest(true);
                    if (quest != null) quest.StartQuest();
                }
            }
        }

        private bool increasecasterlevelcondition()
        {
            bool flag = false;
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            if (quest != null)
            {
                flag = Hero.MainHero.GetExtendedInfo().KnownLores.Count > 1 && quest.ReadyToAdvance;
            }
            if (flag)
            {
                string text = "";
                var culture = Hero.OneToOneConversationHero.Culture.StringId;
                switch (culture)
                {
                    case "empire":
                        {
                            text = "I have learned all that I can and I need more of a challenge, I am ready to harness even greater power and move up within the Order.";
                            break;
                        }
                    case "khuzait":
                        {
                            text = "Master, I have done what you have asked of me. I think I am ready for more advanced rituals and incantations.";
                            if (Hero.MainHero.IsVampire())
                            {
                                text = "I need more power, I need more knowledge. It is time I learn to practice even greater rituals.";
                            }
                            break;
                        }
                    default:
                        text = "You shouldn't see this.";
                        break;
                }
                MBTextManager.SetTextVariable("INCREASE_LEVEL", text);
            }
            return flag;
        }

        private bool specializelorecondition()
        {
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            var info = Hero.MainHero.GetExtendedInfo();
            var possibleLores = new List<LoreObject>();
            foreach (var item in LoreObject.GetAll())
            {
                if (item.ID != "MinorMagic" && !item.DisabledForTrainersWithCultures.Contains(CharacterObject.OneToOneConversationCharacter.Culture.StringId) && !info.HasKnownLore(item.ID)) possibleLores.Add(item);
            }
            bool flag = false;
            if (quest != null && possibleLores.Count > 0)
            {
                flag = info.KnownLores.Count == 1 && info.KnownLores[0].ID == "MinorMagic" && quest.ReadyToAdvance;
                if (!flag && Hero.MainHero.IsVampire() && quest.ReadyToAdvance) flag = true;
            }
            else if(Hero.MainHero.IsVampire() && possibleLores.Count > 0 && info.SpellCastingLevel == SpellCastingLevel.Master) flag = true;
            if (flag)
            {
                string text = "";
                var culture = Hero.OneToOneConversationHero.Culture.StringId;
                switch (culture)
                {
                    case "empire":
                        {
                            text = "I am ready to join the colleges and become a true wizard of the Empire.";
                            break;
                        }
                    case "khuzait":
                        {
                            text = "My Lord, I beseech you to teach me your dark magic. I have learned all that I can on my own and would be a most loyal apprentice to you.";
                            if (Hero.MainHero.IsVampire())
                            {
                                text = "I can feel my dark power continuing to grow, teach me more lest I find myself a new 'tutor'. Hurry on to find your Grimoire, lest I grow thirsty in your absence...";
                            }
                            break;
                        }
                    default:
                        text = "You shouldn't see this.";
                        break;
                }
                MBTextManager.SetTextVariable("SPECALIZE_QUESTION", text);
            }
            return flag;
        }

        private void chooseloreconsequence()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            var lores = LoreObject.GetAll();
            foreach (var item in lores)
            {
                if (item.ID != "MinorMagic" && !item.DisabledForTrainersWithCultures.Contains(CharacterObject.OneToOneConversationCharacter.Culture.StringId) && !Hero.MainHero.GetExtendedInfo().HasKnownLore(item.ID)) list.Add(new InquiryElement(item, item.Name, null));
            }
            var inquirydata = new MultiSelectionInquiryData("Choose Lore", "Choose a lore to specialize in.", list, true, 1, "Confirm", "Cancel", OnChooseLore, OnCancelLore);
            InformationManager.ShowMultiSelectionInquiry(inquirydata, true);
        }

        private void OnChooseLore(List<InquiryElement> obj)
        {
            var choice = obj[0].Identifier as LoreObject;
            if (choice != null)
            {
                Hero.MainHero.AddKnownLore(choice.ID);
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Entry);
                InformationManager.AddQuickInformation(new TextObject("Successfully learned lore: " + choice.Name));
            }
            InformationManager.HideInquiry();
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            if (quest != null) quest.CompleteQuestWithSuccess();
            quest = AdvanceSpellCastingLevelQuest.GetRandomQuest(true);
            if (quest != null) quest.StartQuest();
        }

        private void OnCancelLore(List<InquiryElement> obj)
        {
            InformationManager.HideInquiry();
        }

        private void openbookconsequence()
        {
            var state = Game.Current.GameStateManager.CreateState<SpellBookState>();
            state.IsTrainerMode = true;
            state.TrainerCulture = CharacterObject.OneToOneConversationCharacter.Culture.StringId;
            Game.Current.GameStateManager.PushState(state);
        }

        private bool testresultcondition()
        {
            var result = "Error.";
            if (_testResult == "success")
            {
                result = "Hmm...interesting. It would seem you do have an aptitude, perhaps even potential.";
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Minor);
                var quest = AdvanceSpellCastingLevelQuest.GetRandomQuest(true);
                if(quest!=null) quest.StartQuest();
            }
            else if(_testResult == "failure")
            {
                result = "Pah, it is beyond you. Begone before you waste more of my time.";
            }
            MBTextManager.SetTextVariable("TEST_RESULT", result);
            return true;
        }

        private void determinetestoutcome()
        {
            var rng = MBRandom.RandomFloatRanged(0, 1);
            if (rng <= 0.3f) _testResult = "success";
            else _testResult = "failure";

            //testing
            _testResult = "success";
        }

        private bool spelltrainerstartcondition()
        {
            var partner = CharacterObject.OneToOneConversationCharacter;
            if (partner != null && partner.Occupation == Occupation.Special && partner.HeroObject.Name.Contains("Magister")) return true;
            else return false;
        }

        private void CreateTrainer(Settlement settlement)
        {
            CharacterObject template = null;
            if (settlement.Culture.StringId == "khuzait") template = MBObjectManager.Instance.GetObject<CharacterObject>(_vampireTrainerId);
            else template = MBObjectManager.Instance.GetObject<CharacterObject>(_empireTrainerId);

            if(template != null)
            {
                var hero = HeroCreator.CreateSpecialHero(template, settlement, null, null, 50);
                hero.SupporterOf = settlement.OwnerClan;
                hero.SetName(new TextObject(hero.FirstName.ToString() + " " + template.Name.ToString()), hero.FirstName);
                HeroHelper.SpawnHeroForTheFirstTime(hero, settlement);
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
