using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Messages.FromClient.ToLobbyServer;
using SandBox.Issues.IssueQuestTasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOW_Core.Utilities;

namespace TOW_Core.Quests
{
    public class EngineerTrustQuest : QuestBase
    {
        private static MobileParty _targetParty;

        [SaveableField(1)] private int _destroyedParty = 0;

        [SaveableField(2)] private JournalLog _task1 = null;

        [SaveableField(3)] private JournalLog _task2 = null;
      

        public EngineerTrustQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(
            questId, questGiver, duration, rewardGold)
        {
            SetLogs();

         
        }

        private void SetLogs()
        {
            _task1 = AddDiscreteLog(new TextObject("Find and kill Rudolf, the rogue engineer."),
                new TextObject("killed Rudolf"), _destroyedParty, 1);
            
         
        }
       
        public override TextObject Title => new TextObject("The engineers trust");

        protected override void SetDialogs()
        {

        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnPartyRemovedEvent.AddNonSerializedListener(this, CheckIfPartyWasDestroyedByPlayer);
        }

        
        private void CheckIfPartyWasDestroyedByPlayer(PartyBase obj)
        {
            if (obj.MobileParty == _targetParty)
            {
                TaskSuccessful();
            }
        }

        public void TaskSuccessful()
        {
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
        }

        public override bool IsRemainingTimeHidden { get; }
        
        public static EngineerTrustQuest GetRandomQuest(bool checkForExisting)
        {
            bool exists = false;
            EngineerTrustQuest returnvalue = null;
            if (checkForExisting)
            {
                if (Campaign.Current.QuestManager.Quests.Any(x => x is EngineerTrustQuest && x.IsOngoing))
                    exists = true;
            }

            if (!exists)
            {
                //TODO add random quest from a pool of quests later.
                returnvalue = new EngineerTrustQuest("engineertrustquest", Hero.OneToOneConversationHero,
                    CampaignTime.DaysFromNow(1000), 250);

                var potentialSpawnLocations = new List<Settlement>();
                foreach (var settlement in Campaign.Current.Settlements)
                {
                    if (settlement.Hideout == null)
                        continue;

                    if (settlement.Hideout.MapFaction.Culture.StringId == "mountain_bandits")
                    {
                        potentialSpawnLocations.Add(settlement);
                        
                    }
                }

                var spawnLocation = potentialSpawnLocations.GetRandomElement();
                
                

                var hero = MBObjectManager.Instance.GetObject<CharacterObject>("tor_chaos_lord_factionleader");
                var _enemyLeader = HeroCreator.CreateSpecialHero(hero, spawnLocation, spawnLocation.OwnerClan);
                _enemyLeader.SetNewOccupation(Occupation.Lord);

                
                
                

                /*
                var party = LordPartyComponent.CreateQuestParty(spawnLocation.Position2D, 1f, spawnLocation,
                    new TextObject("Rudolfs Party"), spawnLocation.OwnerClan,
                    spawnLocation.OwnerClan.DefaultPartyTemplate,
                    _enemyLeader);*/
                
              

                var party = LordPartyComponent.CreateLordParty("Rudolfs Party", _enemyLeader, spawnLocation.Position2D,
                    200f, spawnLocation, _enemyLeader);
                
                /*if (party.LeaderHero == null)
                {
                    party.RemovePartyLeader();
                //    party.AddElementToMemberRoster(_enemyLeader.CharacterObject, 1);
                    party.ChangePartyLeader(_enemyLeader);

                    //  party.PartyComponent.Leader.SetCharacterObject(_enemyLeader.CharacterObject);

                }*/
                
                
                

                foreach (var template in spawnLocation.Culture.DefaultPartyTemplate.Stacks)
                {
                    party.AddElementToMemberRoster(template.Character, template.MinValue);
                }

                
                party.ChangePartyLeader(_enemyLeader);
                
                
                TOWCommon.Say(party.LeaderHero.Name.ToString());
                
                party.AddElementToMemberRoster(_enemyLeader.CharacterObject, 1);

                
                party.Aggressiveness = 0f;

                party.IsVisible = true;
                
                party.IgnoreByOtherPartiesTill(CampaignTime.Days(200));

                party.SetPartyUsedByQuest(true);
                
                party.Party.Visuals.SetMapIconAsDirty();
                
                
                
                _targetParty = party;

            //    Campaign.Current.AddMapArrow(new TextObject("Rudolfs position"), party.Position2D,Vec2.Zero,200f,50);
                
                
                //Campaign.Current.VisualTrackerManager.RegisterObject(_targetParty);
                // var party = MobileParty.CreateParty("Rudolfs party",new CustomPartyComponent(), OnPartyCreated);


                //var template = InitializeRogueEngineerPartyTemplate();

                //party.InitializeMobilePartyAroundPosition(template, spawnLocation.Position2D,0.5f,20);

                // party.IsVisible = true;

            }

            return returnvalue;
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
    