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
        private JournalLog _task;

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
            _task = AddDiscreteLog(new TextObject("Use magic 5 times."), new TextObject("Number of casts"), _numberOfCasts, 5);
        }

        public void IncrementCast()
        {
            _numberOfCasts++;
            _task.UpdateCurrentProgress(_numberOfCasts);
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (_task.HasBeenCompleted()) CompleteQuestWithSuccess();
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
