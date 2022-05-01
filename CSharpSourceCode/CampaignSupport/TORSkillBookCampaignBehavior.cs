using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
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

        private string CurrentBook
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
                if (_currentBookObject == null)
                {
                    return;
                }

                _currentBookObject
                        .GetTraits().FindAll(trait => trait.SkillTuple != null)
                        .ForEach(trait => _currentSkillTuples.Add(trait.SkillTuple));

                if (_currentSkillTuples.IsEmpty())
                {
                    ClearCurrentlyReading();
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_currentBook", ref _currentBook);
            dataStore.SyncData("_readingProgress", ref _readingProgress);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
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

        private void ProgressReadingByHours(int hours)
        {
            var hoursToComplete = GetHoursRequiredToComplete();
            var startProgression = _readingProgress.GetValueOrDefault(CurrentBook, 0);
            var endProgression = startProgression + hours;

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
            if (skillTuple.IsAbility)
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
            int startGrantedSkillPoints = (int) (skillTuple.FlatSkillModifier * currentRatio);
            int endGrantedSkillPoints = (int) (skillTuple.FlatSkillModifier * progressedRatio);
            int skillsGained = endGrantedSkillPoints - startGrantedSkillPoints;

            SkillObject skillObject = GetSkillObject(skillTuple.SkillId);
            // TODO(jason): Replace set skill logic with add skill exp so that players
            // also benefit from general exp.
            Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(skillObject, Hero.MainHero.GetSkillValue(skillObject) + skillsGained);
            Hero.MainHero.HeroDeveloper.InitializeSkillXp(skillObject);
            if (skillsGained > 0)
            {
                TOWCommon.Say(string.Format("You gain {0} skill in {1}", skillsGained, skillObject.Name));
            }
        }

        private void ClearCurrentlyReading()
        {
            _currentBook = "";
            _currentSkillTuples.Clear();
            _currentBookObject = null;
        }

        private SkillObject GetSkillObject(string id) 
            => Skills.All.FirstOrDefault(skill => skill.StringId == id);

        private int GetHoursRequiredToComplete() => (int)Math.Ceiling(
                _currentSkillTuples.MaxBy(trait => trait.LearningTime).LearningTime);
    }
}
