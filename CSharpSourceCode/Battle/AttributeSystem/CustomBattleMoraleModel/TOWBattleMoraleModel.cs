using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TOW_Core.Abilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.ObjectDataExtensions.CustomBattleMoralModel
{
    public class TOWBattleMoraleModel : CustomBattleMoraleModel
    {
        public override bool CanPanicDueToMorale(Agent agent)
        {
            if (agent.IsUndead() || agent.IsUnbreakable() || agent.Origin is SummonedAgentOrigin) return false;
            else return base.CanPanicDueToMorale(agent);
        }
    }
    
    
    
}
