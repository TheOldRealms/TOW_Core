using System.Linq;
using NLog.Fluent;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Quests
{
    public class CultistQuest : QuestBase
    {
        [SaveableField(1)] private int _destroyedParty = 0;
        [SaveableField(2)] private JournalLog _task1 = null;
        [SaveableField(3)] private JournalLog _task2 = null;
        [SaveableField(4)] private MobileParty _targetParty = null;
        [SaveableField(5)] private TextObject _title = new TextObject("Runaway Parts");
        
        [SaveableField(6)] private TextObject _targetPartyName = new TextObject("Runaway Parts");
        [SaveableField(7)] private bool _failstate;
        private bool _initAfterReload;

        public override TextObject Title => _title;
        public override bool IsSpecialQuest => true;
        public override bool IsRemainingTimeHidden => false;
        
        public MobileParty GetTargetParty()
        {
            return _targetParty;
        }
        
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this,QuestBattleEndedWithSuccess);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this,QuestBattleEndedWithFail);
        }

        private void QuestBattleEndedWithFail(MapEvent mapEvent)
        {
            if (!mapEvent.IsPlayerMapEvent&& mapEvent.InvolvedParties.Any(party => party.MobileParty == _targetParty)) return;
            if (mapEvent.Winner.MissionSide != mapEvent.PlayerSide)
            {
                CompleteQuestWithFail();
                _targetParty.RemoveParty();
            }
                
        }

        private void QuestBattleEndedWithSuccess(MapEvent mapEvent)
        {
            //the game doesn't give the satisfying bell sound when moved to the other event
            if(!mapEvent.IsPlayerMapEvent || !mapEvent.IsFieldBattle) return;
            if (mapEvent.PartiesOnSide(mapEvent.PlayerSide.GetOppositeSide()).Any(party => party.Party.MobileParty == _targetParty))
            {
                if(mapEvent.Winner.IsMainPartyAmongParties())
                    TaskSuccessful();
                    
               
            }
        }

        private void TaskSuccessful()
        {
            _task1.UpdateCurrentProgress(1);

                if (_task1.HasBeenCompleted() && _task2 == null)
                {
                    _task2 = AddDiscreteLog(new TextObject("I found the thieves, but they did not have the stolen components. I should return to the Master Engineer with the news."),
                        new TextObject("Return to the Master Engineer"), 1, 1);
                }
        }
        
        
        public override void OnFailed()
        {
            base.OnFailed();
            _failstate = true;
        }
        
        public void HandInQuest()
        {
            _task2.UpdateCurrentProgress(1);
            CompleteQuestWithSuccess();
        }
        
        public bool GetQuestFailed()
        {
            return _failstate;
        }

        public CultistQuest(string partyName, string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {
            _targetPartyName = new TextObject(partyName);
            Addlogs();
        }

        protected override void SetDialogs()
        {
        }

        protected override void InitializeQuestOnGameLoad()
        {
            if (!_task1.HasBeenCompleted())
            {
                RegisterQuestSpecificElementsOnGameLoad();
            }
        }

        private void RegisterQuestSpecificElementsOnGameLoad()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this,SetMarkerAfterLoad);
        }

        private void SetMarkerAfterLoad(MobileParty obj)
        {
            if (_initAfterReload) return;
            if (_task1.HasBeenCompleted()) return;
            
            var home = _targetParty.HomeSettlement;
            var hero = _targetParty.LeaderHero;
            var clan = _targetParty.ActualClan;
            _targetParty.RemovePartyLeader();
            _targetParty.RemoveParty();
            _targetParty = null;
            SpawnQuestParty(home.Name);
            
            _initAfterReload = true;
        }

        protected override void OnStartQuest()
        {
            SpawnQuestParty(_targetPartyName);
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
            //hero.SetName(cultistName,cultistName);
            //var party = .CreateBanditParty("questcultistparty", settlement.OwnerClan, settlement.Hideout, true);
            // var party = BanditPartyComponent.CreateBanditParty(settlement.Position2D, 1f, settlement, cultistName, settlement.OwnerClan, settlement.OwnerClan.DefaultPartyTemplate,hero);

           var  party =  QuestPartyComponent.CreateParty(settlement, settlement.OwnerClan);
           
        //  party.Aggressiveness = 0f;

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
    
    public class CultistQuestTypeDefiner : SaveableTypeDefiner
    {
        public CultistQuestTypeDefiner() : base(701793)
        {

        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(CultistQuest), 1);
            
        }
    }
}