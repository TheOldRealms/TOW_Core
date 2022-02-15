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
using TOW_Core.Abilities.SpellBook;
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
        }

        private void OnSessionLaunched(CampaignGameStarter obj)
        {
            AddDialogs(obj);
            foreach(var settlement in Settlement.All)
            {
                if (settlement.IsTown)
                {
                    CreateTrainer(settlement);
                }
            }
        }

        private void AddDialogs(CampaignGameStarter obj)
        {
            obj.AddDialogLine("trainer_start", "start", "choices", "What do you want?", spelltrainerstartcondition, null, 200, null);
            obj.AddPlayerLine("trainer_start", "choices", "magictest", "Test me for magic affinity", ()=> !Hero.MainHero.IsSpellCaster() && _testResult=="", null, 200, null);
            obj.AddDialogLine("trainer_start", "magictest", "testoutcome", "Alright. Here goes. (30% chance)", null, determinetestoutcome, 200, null);
            obj.AddDialogLine("trainer_start", "testoutcome", "choices", "{TEST_RESULT}", testresultcondition, null, 200, null);
            obj.AddPlayerLine("trainer_start", "choices", "openbook", "I would like to learn new spells.", () => Hero.MainHero.IsSpellCaster(), null, 200, null);
            obj.AddDialogLine("trainer_start", "openbook", "choices", "Certainly.", null, openbookconsequence, 200, null);
            obj.AddPlayerLine("trainer_start", "choices", "saygoodbye", "See you later.", null, null, 200, null);
            obj.AddDialogLine("trainer_start", "saygoodbye", "close_window", "Au revoir.", null, null, 200, null);
        }

        private void openbookconsequence()
        {
            var state = Game.Current.GameStateManager.CreateState<SpellBookState>();
            state.IsTrainerMode = true;
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
