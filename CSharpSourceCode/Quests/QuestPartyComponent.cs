using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues.IssueQuestTasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace TOW_Core.Quests
{
    public class QuestPartyComponent : WarPartyComponent
    {
        [SaveableField(10)]
        private TextObject _name;

        [SaveableField(20)]
        private Settlement _homeSettlement;

        [SaveableField(30)]
        private Hero _owner;
        
        public override Hero Leader => _owner;
        public override Hero PartyOwner => _owner;
        public override TextObject Name => _name;
        public override Settlement HomeSettlement => _homeSettlement;
        
        public static MobileParty CreateParty(Settlement settlement, Hero leader, Clan clan, string partyTemplateOverride=null)
        {
            var name=leader.FirstName.ToString() + "'s party";
            PartyTemplateObject partyTemplate = null;
            if(partyTemplateOverride!=null)
            {
                partyTemplate = MBObjectManager.Instance.GetObject<PartyTemplateObject>(partyTemplateOverride);
            }
            
            return MobileParty.CreateParty(leader.StringId + "_questparty_1", new QuestPartyComponent(), delegate (MobileParty mobileParty)
            {
                (mobileParty.PartyComponent as QuestPartyComponent).InitializeQuestPartyProperties(mobileParty, settlement, leader, clan, name, partyTemplate);
            });
        }
        
        private void InitializeQuestPartyProperties(MobileParty mobileParty, Settlement settlement, Hero leader, Clan clan, string name=null, PartyTemplateObject partyTemplate=null)
        {
            var component = mobileParty.PartyComponent as QuestPartyComponent;
            component._owner = leader;
            component._homeSettlement = settlement;
            if(name!=null)
                component._name = new TextObject(name);
            mobileParty.ActualClan = clan;
            mobileParty.Aggressiveness = 0.5f;
            mobileParty.AddElementToMemberRoster(leader.CharacterObject, 1, true);
            if (partyTemplate == null) 
                partyTemplate = clan.DefaultPartyTemplate;
            mobileParty.InitializeMobilePartyAroundPosition(partyTemplate, settlement.Position2D, 10, 0f, 30);
            mobileParty.ItemRoster.Add(new ItemRosterElement(DefaultItems.Grain, 50));
            mobileParty.Ai.SetAIState(AIState.PatrollingAroundLocation);
            mobileParty.SetMovePatrolAroundSettlement(settlement);
            mobileParty.Ai.SetDoNotMakeNewDecisions(true);
            mobileParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
            mobileParty.Party.Visuals.SetMapIconAsDirty();
        }
    }

    public class QuestPartyComponentTypeDefiner : SaveableTypeDefiner
    {
        public QuestPartyComponentTypeDefiner() : base(703799) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(QuestPartyComponent), 1);
        }
    }
}
