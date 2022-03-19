
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Quests
{
    public class InitialEngineerQuest : QuestBase
    {
        private TextObject _title = new TextObject("Hunt down the renegade engineer");
        [SaveableField(1)]
        private JournalLog _task1 = null;
        [SaveableField(2)]
        private MobileParty _targetParty = null;

        public InitialEngineerQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
        {
            Addlogs();
        }

        public override TextObject Title => _title;
        public override bool IsSpecialQuest => true;
        public override bool IsRemainingTimeHidden => false;

        protected override void InitializeQuestOnGameLoad() { }

        protected override void SetDialogs() { }

        protected override void OnStartQuest()
        {
            SpawnQuestParty();
        }

        private void Addlogs()
        {
            _task1 = AddLog(new TextObject("Hunt down target party."));
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

        public static InitialEngineerQuest GetCurrentActiveIfExists()
        {
            InitialEngineerQuest returnvalue = null;
            if (Campaign.Current.QuestManager.Quests.Any(x => x is InitialEngineerQuest && x.IsOngoing))
            {
                returnvalue = Campaign.Current.QuestManager.Quests.FirstOrDefault(x => x is InitialEngineerQuest && x.IsOngoing) as InitialEngineerQuest;
            }
            return returnvalue;
        }

        public static InitialEngineerQuest GetNew()
        {
            return new InitialEngineerQuest("initialengineerquest", Hero.OneToOneConversationHero, CampaignTime.DaysFromNow(30), 1000);
        }
    }

    public class InitialEngineerQuestTypeDefiner : SaveableTypeDefiner
    {
        public InitialEngineerQuestTypeDefiner() : base(701792) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(InitialEngineerQuest), 1);
        }
    }
}*/

