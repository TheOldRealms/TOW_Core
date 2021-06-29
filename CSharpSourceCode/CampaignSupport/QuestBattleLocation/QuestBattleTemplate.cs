using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleTemplate
    {
        [XmlAttribute]
        public string TemplateId { get; set; }
        public string QuestBattleDescription { get; set; }
        public string QuestBattleSolveText { get; set; }
        public string InactiveDescription { get; set; }
        [XmlAttribute]
        public string LeaderHeroCharacterId { get; set; }
        [XmlAttribute]
        public string RewardItemId { get; set; }
        [XmlAttribute]
        public string SceneName { get; set; }
        public List<QuestBattleLocationNpcTuple> TroopTypes { get; set; } = new List<QuestBattleLocationNpcTuple>();

        public class QuestBattleLocationNpcTuple
        {
            [XmlAttribute]
            public string TroopId { get; set; }
            [XmlAttribute]
            public int Count { get; set; }
            [XmlAttribute]
            public bool IsFriendly { get; set; }
        }
    }
}
