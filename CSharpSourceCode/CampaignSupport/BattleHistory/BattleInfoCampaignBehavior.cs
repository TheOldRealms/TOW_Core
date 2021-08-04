using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace TOW_Core.CampaignSupport.BattleHistory
{
    public class BattleInfoCampaignBehavior : CampaignBehaviorBase
    {
        public List<BattleInfo> PlayerBattleHistory = new List<BattleInfo>();

        public override void RegisterEvents()
        {
            
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<BattleInfo>>("PlayerBattleHistory", ref PlayerBattleHistory);
        }
    }
}
