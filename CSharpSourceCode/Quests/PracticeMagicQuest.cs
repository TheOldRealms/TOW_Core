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
    public class PracticeMagicQuest : QuestBase
    {
        [SaveableField(1)]
        private int _numberOfCasts = 0;
        [SaveableField(2)]
        private JournalLog _task1;
        [SaveableField(3)]
        private JournalLog _task2;

        public PracticeMagicQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
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
            _task1.UpdateCurrentProgress(_numberOfCasts);
            CheckCondition();
        }

        public bool ReadyToAdvance => _task1.HasBeenCompleted();

        private void CheckCondition()
        {
            if (_task1.HasBeenCompleted())
            {
                _task2 = AddLog(new TextObject("Visit a spell trainer to advance in caster level."));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PracticeMagicQuest quest &&
                   EqualityComparer<JournalLog>.Default.Equals(_task1, quest._task1);
        }

        public override int GetHashCode()
        {
            int hashCode = -1289645604;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<JournalLog>.Default.GetHashCode(_task1);
            return hashCode;
        }
    }

    public class PracticeMagicQuestTypeDefiner : SaveableTypeDefiner
    {
        public PracticeMagicQuestTypeDefiner() : base(701791) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(PracticeMagicQuest), 1);
        }
    }
}
