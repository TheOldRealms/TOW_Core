using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public class QuestBattleEncounter : LocationEncounter
    {
        public QuestBattleEncounter(Settlement settlement) : base(settlement) { }
    }
}
