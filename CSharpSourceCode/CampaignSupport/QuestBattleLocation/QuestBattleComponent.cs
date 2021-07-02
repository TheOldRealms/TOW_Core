using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleComponent : SettlementComponent
    {
        private QuestBattleTemplate _template = null;
        private Hero _enemyLeader = null;
        public MobileParty QuestOpponentParty { get; private set; } = null;

        public bool IsActive { get; private set; } = true;
        public QuestBattleTemplate QuestBattleTemplate => _template;

        public bool IsQuestBattleUnderway { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            _template = QuestBattleTemplateManager.GetRandomTemplate();
        }

        public void OnQuestBattleComplete(bool withVictory)
        {
            if(QuestOpponentParty != null)
            {
                DestroyPartyAction.Apply(MobileParty.MainParty.Party, QuestOpponentParty);
                QuestOpponentParty = null;
            }
            if(withVictory) IsActive = false;
            if(_enemyLeader != null)
            {
                KillCharacterAction.ApplyByBattle(_enemyLeader, Hero.MainHero);
                _enemyLeader = null;
            }
        }

        public void SpawnDefenderParty()
        {
            MobileParty party = MobileParty.CreateParty(_template.TemplateId + "_party");
            party.HomeSettlement = base.Settlement;
            TroopRoster roster = new TroopRoster(party.Party);
            foreach (var item in _template.TroopTypes)
            {
                if (!item.IsFriendly)
                {
                    roster.AddToCounts(MBObjectManager.Instance.GetObject<CharacterObject>(item.TroopId), item.Count);
                }
            }
            party.InitializeMobileParty(roster, TroopRoster.CreateDummyTroopRoster(), base.Settlement.Position2D, 2);
            party.ActualClan = Clan.All.Where(x => x.StringId == "neutral").FirstOrDefault();
            CharacterObject character = MBObjectManager.Instance.GetObject<CharacterObject>(_template.LeaderHeroCharacterId);
            if (character != null && character.Culture != null)
            {
                party.ActualClan = Clan.All.Where(x => x.Culture.StringId == character.Culture.StringId).FirstOrDefault();
                _enemyLeader = HeroCreator.CreateSpecialHero(character, party.HomeSettlement, party.ActualClan);
                if (_enemyLeader != null)
                {
                    party.AddElementToMemberRoster(_enemyLeader.CharacterObject, 1);
                    party.ChangePartyLeader(_enemyLeader.CharacterObject);
                }
            }
            party.Party.Visuals.SetMapIconAsDirty();
            QuestOpponentParty = party;
        }

        public void StartBattle()
        {
            IsQuestBattleUnderway = true;
        }

        protected override void OnInventoryUpdated(ItemRosterElement item, int count){}

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            base.Deserialize(objectManager, node);
            if (node.Attributes["background_crop_position"] != null)
            {
                base.BackgroundCropPosition = float.Parse(node.Attributes["background_crop_position"].Value);
            }
            if (node.Attributes["background_mesh"] != null)
            {
                base.BackgroundMeshName = node.Attributes["background_mesh"].Value;
            }
            if (node.Attributes["wait_mesh"] != null)
            {
                base.WaitMeshName = node.Attributes["wait_mesh"].Value;
            }
        }
    }
}
