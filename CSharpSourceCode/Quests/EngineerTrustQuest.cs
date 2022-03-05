using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Quests
{
    public class EngineerTrustQuest : QuestBase
    {
        [SaveableField(1)]
        private int _destroyedParty = 0;
        
        [SaveableField(2)]
        private JournalLog _task1 = null;
        
        [SaveableField(3)]
        private JournalLog _task2 = null;
        
        
        
        public EngineerTrustQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {
            SetLogs();
        }

        private void SetLogs()
        {
            _task1 = AddDiscreteLog(new TextObject("Find and kill Rudolf, the rogue engineer."), new TextObject("killed Rudolf"), _destroyedParty, 1);
        }


        public override TextObject Title => new TextObject("The engineers trust");

        protected override void SetDialogs()
        {
            
        }

        public void TaskSuccessful()
        {
            _destroyedParty = 1;
            CheckCondition();
        }
        
        private void CheckCondition()
        {
            if (_task1.HasBeenCompleted() && _task2 == null)
            {
                _task2 = AddLog(new TextObject("Visit the Master Engineer in Nuln."));
            }
        }

        protected override void InitializeQuestOnGameLoad()
        {
        }
        
        public override bool IsRemainingTimeHidden { get; }



        public static EngineerTrustQuest GetRandomQuest(bool checkForExisting)
        {
            bool exists = false;
            EngineerTrustQuest returnvalue = null;
            if (checkForExisting)
            {
                if(Campaign.Current.QuestManager.Quests.Any(x => x is EngineerTrustQuest && x.IsOngoing)) exists = true;
            }
            if (!exists)
            {
                //TODO add random quest from a pool of quests later.
                returnvalue = new EngineerTrustQuest("engineertrustquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(1000), 250);
            }
            return returnvalue;
        }
        
        
    }
    
    public class EngineerTrustQuestTypeDefiner : SaveableTypeDefiner
    {
        public EngineerTrustQuestTypeDefiner() : base(701792) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(EngineerTrustQuest), 1);
        }
    }
}