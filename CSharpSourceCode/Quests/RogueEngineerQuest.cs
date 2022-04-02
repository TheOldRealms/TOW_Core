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
        [SaveableField(5)] private TextObject _targetPartyname = null;
        [SaveableField(6)] private TextObject _partyLeaderName;
        [SaveableField(7)] private string _leaderTemplateId = "";
        [SaveableField(8)] private string _partyTemplateId = "";
        [SaveableField(9)] private string _factionId = "";
        [SaveableField(10)] private string _spawnLocationOwnerId = "";
        [SaveableField(11)] private readonly TextObject _title=null;
        [SaveableField(12)] private TextObject _missionLogText1=null;
        [SaveableField(13)] private TextObject _missionLogTextShort1=null;
        [SaveableField(14)] private TextObject _missionLogText2=null;
        [SaveableField(15)] private TextObject _defeatDialogLine = null;
        [SaveableField(16)] private bool _failstate;
        [SaveableField(17)] private int _rewardXP;
        private bool _initAfterReload;
        private bool _skipImprisonment;
        
        public RogueEngineerQuest(string questId,Hero questGiver, CampaignTime duration, int rewardGold, int rewardXP, string questTitle, string leaderName,
            string leaderTemplate, string targetPartyName, string partyTemplateId, string factionID,
            string spawnLocationOwnerId, string missionLogText1, string missionLogText2=null,
            string missionLogTextShort1=null, string defeatDialogLine=null) : base(
            questId, questGiver, duration, rewardGold)
        {
            _title = new TextObject(questTitle);
            _partyLeaderName = new TextObject(leaderName);
            _targetPartyname = new TextObject(targetPartyName);
            _leaderTemplateId = leaderTemplate;
            _partyTemplateId = partyTemplateId;
            _spawnLocationOwnerId = spawnLocationOwnerId;
            _factionId = factionID;
            _rewardXP = rewardXP;
            _missionLogText1 = new TextObject(missionLogText1);
            _missionLogText2 = new TextObject(missionLogText2);
            _missionLogTextShort1 = new TextObject(missionLogTextShort1);
            _defeatDialogLine = new TextObject(defeatDialogLine);
            
            SetLogs();
        }

        public MobileParty TargetParty => _targetParty;
        public string QuestEnemyLeaderName => _partyLeaderName.ToString();
        public bool FailState => _failstate;

        public int RewardXP => _rewardXP;
        
        public override bool IsSpecialQuest => true;
        public override TextObject Title => _title;
        public override bool IsRemainingTimeHidden => false;
       

        private void SetLogs()
        {
            _task1 = _missionLogTextShort1!=null ? 
                AddDiscreteLog(_missionLogText1, _missionLogTextShort1, _destroyedParty, 1) : 
                AddLog(_missionLogText1);
        }
        
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this,QuestBattleEnded);
            CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this,SkipDialog);
            CampaignEvents.OnPartyRemovedEvent.AddNonSerializedListener(this, KillLeaderFromQuestPartyAfterDialog);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this,QuestBattleEndedWithFail);
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
            var pos = _targetParty.Position2D;
            var name = _targetParty.Name;
            var formerparty = _targetParty;
            _targetParty.SetPartyUsedByQuest(true);
            _targetParty.ChangePartyLeader(null);
            _targetParty.ResetTargetParty();
            
            SpawnQuestParty(hero, home, clan,name);
            formerparty.RemoveParty();
            _targetParty.Party.Visuals.SetMapIconAsDirty();
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
        
        private void QuestBattleEndedWithFail(MapEvent mapEvent)
        {
            if (mapEvent.Winner == null) return;
            if (mapEvent.IsPlayerMapEvent|| !mapEvent.InvolvedParties.Any(party => party.MobileParty == _targetParty)) return;
            if (mapEvent.Winner.MissionSide == mapEvent.PlayerSide) return;
            CompleteQuestWithFail();
            _targetParty.RemoveParty();
        }
        
        private void SkipDialog()
        {
            if (!_targetParty.IsActive) return;
            if (!_skipImprisonment) return;
            if (Campaign.Current.CurrentConversationContext != ConversationContext.CapturedLord) return;
            Campaign.Current.ConversationManager.EndConversation();
            Campaign.Current.ConversationManager.AddDialogLineMultiAgent("start", "start", "rogueengineer_playerafterbattle", _defeatDialogLine, ()=> _skipImprisonment, RemoveSkip, 0,1, 200, null);
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
            
            SpawnQuestParty(_partyLeaderName,_targetPartyname,null);
        }

        public void TaskSuccessful()
        {
            _task1.UpdateCurrentProgress(1);

            if (_task1.HasBeenCompleted() && _task2 == null)
            {
                _task2 = AddLog(_missionLogText2);
            }
        }
        
        public void HandInQuest()
        {
            _task2.UpdateCurrentProgress(1);
            CompleteQuestWithSuccess();
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
        
        public static RogueEngineerQuest GetNew(
            string questId, 
            int questRewardGold,
            int questRewardXp, 
            string questTitle, 
            string leaderName, 
            string leaderTemplate, 
            string targetPartyName, 
            string partyTemplate,
            string factionId, 
            string spawnLocationOwnerId,
            string missionLogText1,
            string missionLogTextShort1,
            string missionLogText2,
            string defeatDialog)
        {
            return new RogueEngineerQuest(questId, 
                Hero.OneToOneConversationHero,
                CampaignTime.DaysFromNow(30),
                questRewardGold,
                questRewardXp,
                questTitle,
                leaderName,
                leaderTemplate,
                targetPartyName,
                partyTemplate,
                factionId,
                spawnLocationOwnerId,
                missionLogText1, missionLogText2,missionLogTextShort1,defeatDialog
                );
        }
        
        
        
        private void SpawnQuestParty(Hero hero, Settlement spawnSettlement, Clan clan, TextObject name)
        {
            var party = QuestPartyComponent.CreateParty(spawnSettlement, hero, clan);
            party.Party.Visuals.SetVisualVisible(true);
            party.Party.Visuals.SetMapIconAsDirty();
            party.SetCustomName(name); 
            party.SetPartyUsedByQuest(true);
            AddTrackedObject(party);
            _targetParty = party;
        }
        
        
        
        private void SpawnQuestParty(TextObject heroNameOverride=null, TextObject partyNameOverride=null, TextObject spawnLocationOverride=null)
        {
            //this is intended as a quick fix, if we dont  want a full random spawning
            var settlement = spawnLocationOverride == null ? 
                Settlement.All.FirstOrDefault(x => x.IsHideout && x.Culture.StringId == _spawnLocationOwnerId) : 
                Settlement.All.FirstOrDefault(x => x.Name ==spawnLocationOverride);
            
            var leaderTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(_leaderTemplateId);
            var faction =  Campaign.Current.Factions.FirstOrDefault(x => x.StringId.ToString() == _factionId);
            var factionClan = (Clan)faction;
            var hero = HeroCreator.CreateSpecialHero(leaderTemplate, settlement, factionClan , null, 45);
            if(heroNameOverride!=null)hero.SetName(heroNameOverride, heroNameOverride);
            var party = QuestPartyComponent.CreateParty(settlement, hero, factionClan, _partyTemplateId);
            if(partyNameOverride!=null)party.SetCustomName(partyNameOverride);
            party.Aggressiveness = 0f;
            party.IgnoreByOtherPartiesTill(CampaignTime.Never);
            party.SetPartyUsedByQuest(true);
            _targetParty = party;

        }
        
    }
    
    
    
    public class RogueEngineerQuestTypeDefiner : SaveableTypeDefiner
    {
        public RogueEngineerQuestTypeDefiner() : base(701792)
        {

        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(RogueEngineerQuest), 1);
        }
    }
}
    
    

    