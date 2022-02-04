using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Reads out ExtendedInfoManager during Combat
    /// </summary>
    public class ExtendedInfoMissionLogic : MissionLogic
    {
        public override void OnMissionDeactivate()
        {
            base.OnMissionDeactivate();
            SpellBlowInfoManager.Clear();
        }
    }
}