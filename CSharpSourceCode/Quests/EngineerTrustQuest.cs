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
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Quests
{
    public class EngineerTrustQuest : QuestBase
    {
        private TextObject _title = new TextObject("Hunt down the Engineer");
        
        private static Hero enemyBoss;

        [SaveableField(1)] private int _destroyedParty = 0;

        [SaveableField(2)] private JournalLog _task1 = null;

        [SaveableField(3)] private JournalLog _task2 = null;

        [SaveableField(4)] private MobileParty _targetParty = null;
        
         
         
        
        public EngineerTrustQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(
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
            _task1 = AddDiscreteLog(new TextObject("Find and kill Rudolf, the rogue engineer."),
                new TextObject("killed Rudolf"), _destroyedParty, 1);
            
        }

        protected override void SetDialogs()
        {

        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            
            // CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this,Kill2);
           // CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, SetMarkerAfterLoad);
          CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this,ended);
          //CampaignEvents.PlayerMetCharacter.AddNonSerializedListener(this,SkipDialog);
          CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this,SkipDialog);
         // CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this,SkipDialog);
          CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, KillLeader);
         CampaignEvents.OnPartyRemovedEvent.AddNonSerializedListener(this, CheckIfPartyWasDestroyedByPlayer);
        }




        private void RegisterQuestSpecificElementsOnGameLoad()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this,SetMarkerAfterLoad);
        }
        private void SetMarkerAfterLoad(CampaignGameStarter campaignGameStarter)
        {
            if (!_task1.HasBeenCompleted())
            {
     //           _targetParty.Party.Visuals.SetMapIconAsDirty();
            }
        }
        
        private void ended(MapEvent mapEvent)
        {
            if (mapEvent.IsPlayerMapEvent)
            {
                foreach (var party in mapEvent.PartiesOnSide(mapEvent.PlayerSide.GetOppositeSide()))
                {
                    if (party.Party.MobileParty == _targetParty)
                    {
                        _skipImprisonment = true;
                       break;
                    }
                }
                
            }
        }
        private void SkipDialog()
        {
            if(_targetParty.IsActive)
                
                if (_skipImprisonment)
                {
                    if (Campaign.Current.CurrentConversationContext == ConversationContext.CapturedLord)
                    {
                        Campaign.Current.ConversationManager.EndConversation();
                        Campaign.Current.ConversationManager.AddDialogLineMultiAgent("start", "start", "close_window",
                            new TextObject("*dies*"), ()=> _skipImprisonment, null, 0,1, 200, null);
                        Campaign.Current.ConversationManager.ClearCurrentOptions();
                    }
                }
        }
        
        
        private void KillLeader(IMission Imission)
        {

            var mission = (Mission)Imission;
            
            
            
            Hero enemyleader = mission.Teams.PlayerEnemy.GeneralAgent.GetHero();

            
            
            if (enemyleader.PartyBelongedTo == _targetParty)
                KillCharacterAction.ApplyByRemove(enemyleader);
              
        }
        
        
        private void CheckIfPartyWasDestroyedByPlayer(PartyBase obj)
        {
            var playerparty = Campaign.Current.MainParty.Party;
            
            
            if (obj.MobileParty == _targetParty)
            {
               KillCharacterAction.ApplyByRemove(obj.Owner,false);
                TaskSuccessful();
            }
        }
        
        protected override void OnStartQuest()
        {
            SpawnQuestParty();
        }

        public void TaskSuccessful()
        {
            _skipImprisonment = false;
            _task1.UpdateCurrentProgress(1);
            CheckCondition();
        }
        
        public void HandInQuest()
        {
            _task2.UpdateCurrentProgress(1);
            CompleteQuestWithSuccess();
        }

        private void CheckCondition()
        {
            if (_task1.HasBeenCompleted() && _task2 == null)
            {
                _task2 = AddLog(new TextObject("Visit the Master Engineer in Nuln."));
            }
        }

        public void ReturnDialog()
        {
            
        }

        protected override void InitializeQuestOnGameLoad()
        {
            if (!_task1.HasBeenCompleted())
            {
                RegisterQuestSpecificElementsOnGameLoad();
            }
        }
        
        
        public static EngineerTrustQuest GetCurrentActiveIfExists()
        {
            EngineerTrustQuest returnvalue = null;
            if (Campaign.Current.QuestManager.Quests.Any(x => x is EngineerTrustQuest && x.IsOngoing))
            {
                returnvalue = Campaign.Current.QuestManager.Quests.FirstOrDefault(x => x is EngineerTrustQuest && x.IsOngoing) as EngineerTrustQuest;
            }
            return returnvalue;
        }
        
        public static EngineerTrustQuest GetNew()
        {
            return new EngineerTrustQuest("initialengineerquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(30), 1000);
        }

        private void SpawnQuestParty()
        {
            var settlement = Settlement.All.FirstOrDefault(x => x.IsHideout && x.Culture.StringId == "mountain_bandits");
            var template = MBObjectManager.Instance.GetObject<CharacterObject>("tor_engineerquesthero");
            var clan = Clan.FindFirst(x => x.StringId == "empire_deserter_clan_1");
            var leaderhero = HeroCreator.CreateSpecialHero(template, settlement, clan, null, 45);
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
            AddClassDefinition(typeof(EngineerTrustQuest), 1);
        }
    }
}
    
    

    