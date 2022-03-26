using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Messages.FromClient.ToLobbyServer;
using SandBox.Issues.IssueQuestTasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.SandBox.Issues.IssueQuestTasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Abilities;
using TOW_Core.CampaignSupport.TownBehaviours;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Quests
{
    public class RogueEngineerQuest : QuestBase
    {
        [SaveableField(1)] private int _destroyedParty = 0;
        [SaveableField(2)] private JournalLog _task1 = null;
        [SaveableField(3)] private JournalLog _task2 = null;
        [SaveableField(4)] private MobileParty _targetParty = null;
        [SaveableField(5)] private string _enemyHeroName = "Goswin";
        [SaveableField(6)] private TextObject _title = new TextObject("Hunt down the Engineer");
        [SaveableField(7)] private bool _failstate;
        private bool _initAfterReload;


        public RogueEngineerQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(
            questId, questGiver, duration, rewardGold)
        {
            SetLogs();
        }
        
        public override bool IsSpecialQuest => true;
        public override TextObject Title => _title;
        public override bool IsRemainingTimeHidden => false;
        private bool _skipImprisonment;

        private void SetLogs()
        {
            _task1 = AddDiscreteLog(new TextObject("Find and kill"+_enemyHeroName+ ", the rogue engineer."),
                new TextObject("killed "+_enemyHeroName), _destroyedParty, 1);
        }
        
        public string GetRogueEngineerName()
        {
            return _enemyHeroName;
        }

        public bool GetQuestFailed()
        {
            return _failstate;
        }
        
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this,QuestBattleEnded);
            CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this,SkipDialog);
            CampaignEvents.OnPartyRemovedEvent.AddNonSerializedListener(this, KillLeaderFromQuestPartyAfterDialog);
        }
        
        private void RegisterQuestSpecificElementsOnGameLoad()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this,SetMarkerAfterLoad);
        }
        
        private void SetMarkerAfterLoad(MobileParty mobileParty)
        {
            if (_initAfterReload) return;
            if (_task1.HasBeenCompleted()) return;
            
            var home = _targetParty.HomeSettlement;
            var hero = _targetParty.LeaderHero;
            var clan = _targetParty.ActualClan;
            _targetParty.RemovePartyLeader();
            _targetParty.RemoveParty();
            _targetParty = null;
            SpawnQuestParty(hero.Name,home,clan);
            _initAfterReload = true;
        }
        
        private void QuestBattleEnded(MapEvent mapEvent)
        {
            if (!mapEvent.IsPlayerMapEvent || !mapEvent.IsFieldBattle) return;
            if (mapEvent.PartiesOnSide(mapEvent.PlayerSide.GetOppositeSide()).Any(party => party.Party.MobileParty == _targetParty))
            {
                _skipImprisonment = true;
                TaskSuccessful();
            }
        }
        
        private void SkipDialog()
        {
            if (!_targetParty.IsActive) return;
            if (!_skipImprisonment) return;
            if (Campaign.Current.CurrentConversationContext != ConversationContext.CapturedLord) return;
            Campaign.Current.ConversationManager.EndConversation();
            Campaign.Current.ConversationManager.AddDialogLineMultiAgent("start", "start", "rogueengineer_playerafterbattle", new TextObject("*You have no idea what you are interfering with...*"), ()=> _skipImprisonment, RemoveSkip, 0,1, 200, null);
            Campaign.Current.ConversationManager.ClearCurrentOptions();
        }
        
        private void RemoveSkip()
        {
            _skipImprisonment = false;
        }
        
        private void KillLeaderFromQuestPartyAfterDialog(PartyBase obj)
        {
            if (obj.MobileParty == _targetParty)
            {
                KillCharacterAction.ApplyByRemove(obj.Owner,false);
            }
        }
        
        protected override void OnStartQuest()
        {
            SpawnQuestParty(new TextObject(_enemyHeroName));
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
        }

        private void CheckCondition()
        {
            
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
        
        public override void OnFailed()
        {
            base.OnFailed();
            _failstate = true;
        }

        public static RogueEngineerQuest GetCurrentActiveIfExists()
        {
            RogueEngineerQuest returnvalue = null;
            if (Campaign.Current.QuestManager.Quests.Any(x => x is RogueEngineerQuest && x.IsOngoing))
            {
                returnvalue = Campaign.Current.QuestManager.Quests.FirstOrDefault(x => x is RogueEngineerQuest && x.IsOngoing) as RogueEngineerQuest;
            }
            return returnvalue;
        }
        
        public static RogueEngineerQuest GetNew()
        {
            return new RogueEngineerQuest("rogueengineerquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(30), 1000);
        }
        
        private void SpawnQuestParty(TextObject heroName=null, Settlement location=null,Clan ownerClan=null)
        {
            Settlement settlement = location ?? Settlement.All.FirstOrDefault(x => x.IsHideout && x.Culture.StringId == "mountain_bandits");
            var template = MBObjectManager.Instance.GetObject<CharacterObject>("tor_engineerquesthero");
            Clan clan = ownerClan?? Clan.FindFirst(x => x.StringId == "empire_deserter_clan_1");
            var leaderhero = HeroCreator.CreateSpecialHero(template, settlement, clan, null, 45);
            if(heroName!=null)leaderhero.SetName(heroName, heroName);
            var party = QuestPartyComponent.CreateParty(settlement, leaderhero, clan);
            party.SetPartyUsedByQuest(true);
            AddTrackedObject(party);
            _targetParty = party;
        }
    }
    
    public class EngineerTrustQuestTypeDefiner : SaveableTypeDefiner
    {
        public EngineerTrustQuestTypeDefiner() : base(701792)
        {

        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(RogueEngineerQuest), 1);
        }
    }
}
    
    

    