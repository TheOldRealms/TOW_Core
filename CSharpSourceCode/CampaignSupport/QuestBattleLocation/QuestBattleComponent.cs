using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleComponent : SettlementComponent
    {
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
