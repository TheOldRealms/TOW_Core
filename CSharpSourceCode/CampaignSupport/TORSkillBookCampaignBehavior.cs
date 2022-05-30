using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.Abilities;
using TOW_Core.Items;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.CampaignSupport
{
    class TORSkillBookCampaignBehavior : CampaignBehaviorBase
    {
        private string _currentBook = "";
        /** Maps book item id to hours read in whole numbers. */
        private Dictionary<string, int> _readingProgress = new Dictionary<string, int>();
        private ItemObject _currentBookObject;
        private List<SkillTuple> _currentSkillTuples = new List<SkillTuple>();
        private static TORSkillBookCampaignBehavior _instance;

        public static TORSkillBookCampaignBehavior Instance => _instance;

        public TORSkillBookCampaignBehavior() : base()
        {
            _instance = this;
        }

        public string CurrentBook
        {
            get
            {
                return _currentBook;
            }
            set
            {
                _currentBook = value;
                _currentSkillTuples.Clear();
                _currentBookObject = MBObjectManager.Instance.GetObject<ItemObject>(CurrentBook);

                _currentSkillTuples = GetSkillTuples(_currentBookObject);

                if (_currentSkillTuples.IsEmpty())
                {
                    ClearCurrentlyReading();
                }
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunchedEvent);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_currentBook", ref _currentBook);
            dataStore.SyncData("_readingProgress", ref _readingProgress);
        }

        private void OnSessionLaunchedEvent(CampaignGameStarter obj)
        {
            CurrentBook = _currentBook;
        }

        private void HourlyTick()
        {
            // Only allow reading while player party is still. and a book is selected.
            if (!MobileParty.MainParty.ComputeIsWaiting()
                || CurrentBook.IsEmpty())
            {
                return;
            }

            // If the item isn't currently in the player's inventory, clear data.
            if (!MobileParty.MainParty.ItemRoster.Any(item => item.EquipmentElement.Item.StringId.Equals(CurrentBook)))
            {
                ClearCurrentlyReading();
                return;
            }

            if (_readingProgress.GetValueOrDefault(CurrentBook, 0) == GetHoursRequiredToComplete())
            {
                return;
            }

            ProgressReadingByHours(1);
        }

        public bool IsSkillBook(ItemObject book)
        {
            return book.GetTraits().Any(trait => trait.SkillTuple != null);
        }

        /** 
         * Returns true if there is any skill xp to collect or any unlearned
         * abilities to collect.
         */
        public bool IsBookUseful(ItemObject book)
        {
            var skillTuples = GetSkillTuples(book);

            return GetHoursLeftToRead(book) > 0
                && skillTuples.Any(skillTuple 
                    => !skillTuple.IsAbility 
                    || IsUsefulForAbility(skillTuple)
            );
        }

        private void ProgressReadingByHours(int hours)
        {
            var hoursToComplete = GetHoursRequiredToComplete();
            if (!IsBookUseful(_currentBookObject))
            {
                TOWCommon.Say(String.Format("You feel as if there is nothing left to gain from reading {0}", _currentBookObject.Name));
                _readingProgress[CurrentBook] = hoursToComplete;
                return;
            }

            var startProgression = _readingProgress.GetValueOrDefault(CurrentBook, 0);
            var endProgression = Math.Min(startProgression + hours, hoursToComplete);
            // If we ever decide to progress reading by > 1 hour, we should cap
            // progression or too many skill points might be allocated.
            hours = endProgression - startProgression;

            _currentSkillTuples.ForEach(
                skillTuple => CalculateSkillTupleProgression(skillTuple, startProgression, hours));

            _readingProgress[CurrentBook] =
                Math.Min(endProgression, hoursToComplete);
            TOWCommon.Say(
                String.Format("Reading for {0} has progressed to {1:0.00}%",
                _currentBookObject.Name, 100 * ((float) endProgression / hoursToComplete)));
        }

        private void CalculateSkillTupleProgression(SkillTuple skillTuple, int currentProgression, int hours)
        {
            if (currentProgression >= skillTuple.LearningTime)
                return;

            // If this is an ability scroll, we don't need to do complex calculations
            if (skillTuple.IsAbility && IsUsefulForAbility(skillTuple))
            {
                var abilityTemplate = AbilityFactory.GetTemplate(skillTuple.SkillId);
                if (currentProgression + hours >= skillTuple.LearningTime
                    && abilityTemplate != null
                    && !Hero.MainHero.HasAbility(skillTuple.SkillId))
                {
                    Hero.MainHero.AddAbility(skillTuple.SkillId);
                    TOWCommon.Say(String.Format("{0} has gained the {1} ability!", 
                        Hero.MainHero.Name, abilityTemplate.Name));
                }
                return;
            }

            // Figure out how many skill points should be allocated for the # hours
            // passed.
            float currentRatio = currentProgression / skillTuple.LearningTime;
            float progressedRatio = Math.Min(currentProgression + hours, skillTuple.LearningTime) / skillTuple.LearningTime;
            float startGrantedExp = (skillTuple.SkillExp * currentRatio);
            float endGrantedExp = (skillTuple.SkillExp * progressedRatio);
            float expGained = endGrantedExp - startGrantedExp;

            SkillObject skillObject = GetSkillObject(skillTuple.SkillId);
            Hero.MainHero.AddSkillXp(skillObject, expGained);
        }

        private void ClearCurrentlyReading()
        {
            _currentBook = "";
            _currentSkillTuples.Clear();
            _currentBookObject = null;
        }

        private List<SkillTuple> GetSkillTuples(ItemObject item)
        {
            List<SkillTuple> skillTuples = new List<SkillTuple>();
            if (item is null)
            {
                return skillTuples;
            }
            
            item
                .GetTraits().FindAll(trait => trait.SkillTuple != null)
                .ForEach(trait => skillTuples.Add(trait.SkillTuple));
            return skillTuples;
        }

        private SkillObject GetSkillObject(string id) 
            => Skills.All.FirstOrDefault(skill => skill.StringId == id);

        private int GetHoursLeftToRead(ItemObject book)
        {
            return GetHoursRequiredToComplete(book)
                - _readingProgress.GetValueOrDefault(book.StringId, 0);
        }

        private bool IsUsefulForAbility(SkillTuple abilityTuple)
        {
            return !Hero.MainHero.HasAbility(abilityTuple.SkillId);
        }

        private int GetHoursRequiredToComplete() => GetHoursRequiredToComplete(_currentSkillTuples);

        private int GetHoursRequiredToComplete(ItemObject book)
        {
            var skillTuples = GetSkillTuples(book);
            return GetHoursRequiredToComplete(skillTuples);
        }

        private int GetHoursRequiredToComplete(List<SkillTuple> skillTuples)
        {
            SkillTuple maxLearningTime = skillTuples.MaxBy(trait => trait.LearningTime);
            if (maxLearningTime is null)
            {
                return 0;
            }

            return (int)Math.Ceiling(maxLearningTime.LearningTime);
        }
    }
}
