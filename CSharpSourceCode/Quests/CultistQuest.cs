using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Quests
{
    public class CultistQuest : QuestBase
    {
        [SaveableField(1)] private int _destroyedParty = 0;
        [SaveableField(2)] private JournalLog _task1 = null;
        [SaveableField(3)] private JournalLog _task2 = null;
        [SaveableField(3)] private MobileParty _targetParty = null;
        [SaveableField(4)] private TextObject _title = new TextObject("Runaway Parts");
        
        [SaveableField(4)] private TextObject _cultistName = new TextObject("Runaway Parts");
        [SaveableField(5)] private bool _failstate;

        public override TextObject Title => _title;
        public override bool IsSpecialQuest => true;
        public override bool IsRemainingTimeHidden => false;
        
        
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this,QuestBattleEnded);
        }

        private void QuestBattleEnded(MapEvent mapEvent)
        {
            if(!mapEvent.IsPlayerMapEvent || !mapEvent.IsFieldBattle) return;
            if (mapEvent.PartiesOnSide(mapEvent.PlayerSide.GetOppositeSide()).Any(party => party.Party.MobileParty == _targetParty))
            {
                TaskSuccessful();
            }
        }
        
        public void TaskSuccessful()
        {
            _task1.UpdateCurrentProgress(1);

                if (_task1.HasBeenCompleted() && _task2 == null)
                {
                    _task2 = AddLog(new TextObject("Visit the Master Engineer in Nuln."));
                }
        }
        
        
        public void HandInQuest()
        {
            _task2.UpdateCurrentProgress(1);
            CompleteQuestWithSuccess();
            
           //Campaign.Current.MainParty.LeaderHero.AddSkillXp(SkillLevelingManager.Add);
        }
        public bool GetQuestFailed()
        {
            return _failstate;
        }

        public CultistQuest(string partyName, string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {
            _cultistName = new TextObject(partyName);
            Addlogs();
        }

        protected override void SetDialogs()
        {
        }

        protected override void InitializeQuestOnGameLoad()
        {
        }
        
        protected override void OnStartQuest()
        {
            SpawnQuestParty(_cultistName);
        }

        private void Addlogs()
        {
            _task1 = AddDiscreteLog(new TextObject("The Master Engineer has tasked me with hunting down thieving runaways, I should find them and bring back what they stole."),
                new TextObject("Track down runaway thieves"), _destroyedParty, 1);
        }
        
        private void SpawnQuestParty(TextObject cultistName)
        {
            var settlement = Settlement.All.FirstOrDefault(x => x.IsHideout && x.Culture.StringId == "forest_bandits");
            var template = MBObjectManager.Instance.GetObject<CharacterObject>("tor_empire_deserter_lord_0");
            var hero = HeroCreator.CreateSpecialHero(template, settlement, settlement.OwnerClan, null, 45);
            var party = CustomPartyComponent.CreateQuestParty(settlement.Position2D, 1f, settlement, cultistName, settlement.OwnerClan, settlement.OwnerClan.DefaultPartyTemplate,hero);
            
            party.SetPartyUsedByQuest(true);
            AddTrackedObject(party);
            _targetParty = party;
        }
        
        public static CultistQuest GetCurrentActiveIfExists()
        {
            CultistQuest returnvalue = null;
            if (Campaign.Current.QuestManager.Quests.Any(x => x is CultistQuest && x.IsOngoing))
            {
                returnvalue = Campaign.Current.QuestManager.Quests.FirstOrDefault(x => x is CultistQuest && x.IsOngoing) as CultistQuest;
            }
            return returnvalue;
        }

        public static CultistQuest GetNew(string QuestPartyName = null)
        {
            if (QuestPartyName != null)
            {
                return new CultistQuest(QuestPartyName, "chaoscultistquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(30), 1000);
            }
            else
            {
                return new CultistQuest("Cultists", "chaoscultistquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(30), 1000);
            }
            
        }
        
        
        
        
    }
}