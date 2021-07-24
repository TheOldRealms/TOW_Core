﻿using SandBox;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomBattleMoralModel
{
    public class CustomBattleMoralModel
    {
        public class TOWCampaignBattleMoraleModel : DefaultBattleMoraleModel
        {
            public override bool CanPanicDueToMorale(Agent agent)
            {
                if (agent.IsUndead()) return false;
                else return base.CanPanicDueToMorale(agent);
            }
        }

    }
}