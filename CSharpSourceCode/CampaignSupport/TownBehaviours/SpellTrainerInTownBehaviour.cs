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
            obj.AddDialogLine("trainer_start", "start", "choices", "What do you want?", spelltrainerstartcondition, null, 200, null);
            obj.AddPlayerLine("trainer_test", "choices", "magictest", "Test me for magic affinity", ()=> !Hero.MainHero.IsSpellCaster() && _testResult=="", null, 200, null);
            obj.AddDialogLine("trainer_testoutcome", "magictest", "testoutcome", "Alright. Here goes. (30% chance)", null, determinetestoutcome, 200, null);
            obj.AddDialogLine("trainer_testresult", "testoutcome", "start", "{TEST_RESULT}", testresultcondition, null, 200, null);
            obj.AddPlayerLine("trainer_learnspells", "choices", "openbook", "I would like to learn new spells.", () => Hero.MainHero.IsSpellCaster(), null, 200, null);
            obj.AddDialogLine("trainer_afterlearnspells", "openbook", "start", "Certainly.", null, openbookconsequence, 200, null);
            obj.AddPlayerLine("trainer_howtoadvance", "choices", "getquest", "How do I get access to higher tier spells?", () => AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists() == null && Hero.MainHero.GetExtendedInfo().SpellCastingLevel < SpellCastingLevel.Master && Hero.MainHero.GetExtendedInfo().SpellCastingLevel > SpellCastingLevel.None, null, 200, null);
            obj.AddDialogLine("trainer_getadvancequest", "getquest", "start", "You need to prove yourself.", null, advancequestconsequence, 200, null);
            obj.AddPlayerLine("trainer_specialize", "choices", "specializelore", "I would like to specialize in an advanced lore of magic.", specializelorecondition, null, 200, null);
            obj.AddDialogLine("trainer_chooselore", "specializelore", "start", "Choose wisely. Your choice is final and will lock you out of all other lores.", null, chooseloreconsequence, 200, null);
            obj.AddPlayerLine("trainer_increaselevel", "choices", "increasecasterlevel", "I am ready to step up in the ranks of the order (Increase caster level).", increasecasterlevelcondition, null, 200, null);
            obj.AddDialogLine("trainer_confirmnewlevel", "increasecasterlevel", "start", "You have proven that you have a firm grasp of magic. Welcome to the new ranks.", null, increasecasterlevelconsequence, 200, null);
            obj.AddPlayerLine("trainer_playergoodbye", "choices", "saygoodbye", "See you later.", null, null, 200, null);
            obj.AddDialogLine("trainer_goodbye", "saygoodbye", "close_window", "Au revoir.", null, null, 200, null);
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
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            if (quest != null)
            {
                return Hero.MainHero.GetExtendedInfo().KnownLores.Count > 1 && quest.ReadyToAdvance;
            }
            else return false;
        }

        private bool specializelorecondition()
        {
            var quest = AdvanceSpellCastingLevelQuest.GetCurrentActiveIfExists();
            if (quest != null)
            {
                return Hero.MainHero.GetExtendedInfo().KnownLores.Count == 1 && Hero.MainHero.GetExtendedInfo().KnownLores[0].ID == "MinorMagic" && quest.ReadyToAdvance;
            }
            else return false;
        }

        private void chooseloreconsequence()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            var lores = LoreObject.GetAll();
            foreach (var item in lores)
            {
                if (item.ID != "MinorMagic" && !item.DisabledForTrainersWithCultures.Contains(CharacterObject.OneToOneConversationCharacter.Culture.StringId)) list.Add(new InquiryElement(item, item.Name, null));
            }
            var inquirydata = new MultiSelectionInquiryData("Choose Lore", "Choose wisely. You can only choose a single lore to specialize in. This choice is final.", list, true, 1, "Confirm", "Cancel", OnChooseLore, OnCancelLore);
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
                result = "It seems you are showing signs of magical aptitude.";
                Hero.MainHero.AddAttribute("AbilityUser");
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Minor);
                var quest = AdvanceSpellCastingLevelQuest.GetRandomQuest(true);
                if(quest!=null) quest.StartQuest();
            }
            else if(_testResult == "failure")
            {
                result = "It looks like the arcane arts are beyond your reach.";
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
