using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Quests
{
    public class AdvanceSpellCastingLevelQuest : QuestBase
    {
        [SaveableField(1)]
        private int _numberOfCasts = 0;
        [SaveableField(2)]
        private JournalLog _task1 = null;
        [SaveableField(3)]
        private JournalLog _task2 = null;

        public AdvanceSpellCastingLevelQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {
            SetLogs();
        }

        public override TextObject Title => new TextObject("Practice Spellcasting");

        public override bool IsRemainingTimeHidden => false;

        protected override void InitializeQuestOnGameLoad() { }

        protected override void SetDialogs() { }

        public override bool IsSpecialQuest => true;

        private void SetLogs()
        {
            _task1 = AddDiscreteLog(new TextObject("Use magic 5 times."), new TextObject("Number of casts"), _numberOfCasts, 5);
        }

        public void IncrementCast()
        {
            _numberOfCasts++;
            if(!_task1.HasBeenCompleted()) _task1.UpdateCurrentProgress(_numberOfCasts);
            CheckCondition();
        }

        public bool ReadyToAdvance => _task1.HasBeenCompleted();

        private void CheckCondition()
        {
            if (_task1.HasBeenCompleted() && _task2 == null)
            {
                _task2 = AddLog(new TextObject("Visit a spell trainer to advance in caster level."));
            }
        }

        public static AdvanceSpellCastingLevelQuest GetRandomQuest(bool checkForExisting)
        {
            bool exists = false;
            AdvanceSpellCastingLevelQuest returnvalue = null;
            if (checkForExisting)
            {
                if(Campaign.Current.QuestManager.Quests.Any(x => x is AdvanceSpellCastingLevelQuest && x.IsOngoing)) exists = true;
            }
            if (!exists)
            {
                //TODO add random quest from a pool of quests later.
                returnvalue = new AdvanceSpellCastingLevelQuest("practicemagic", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(1000), 100);
            }
            return returnvalue;
        }

        public static AdvanceSpellCastingLevelQuest GetCurrentActiveIfExists()
        {
            AdvanceSpellCastingLevelQuest returnvalue = null;
            if(Campaign.Current.QuestManager.Quests.Any(x => x is AdvanceSpellCastingLevelQuest && x.IsOngoing))
            {
                returnvalue = Campaign.Current.QuestManager.Quests.FirstOrDefault(x => x is AdvanceSpellCastingLevelQuest && x.IsOngoing) as AdvanceSpellCastingLevelQuest;
            }
            return returnvalue;
        }
    }

    public class PracticeMagicQuestTypeDefiner : SaveableTypeDefiner
    {
        public PracticeMagicQuestTypeDefiner() : base(701791) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(AdvanceSpellCastingLevelQuest), 1);
        }
    }
}
