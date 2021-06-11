using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;

namespace TOW_Core.Battle.AttributeSystem.CustomBattleMoralModel
{
    public class TOWBattleMoraleModel : CustomBattleMoraleModel
    {
        public override bool CanPanicDueToMorale(Agent agent)
        {
            if (agent.IsUndead()) return false;
            else return base.CanPanicDueToMorale(agent);
        }
    }
}
