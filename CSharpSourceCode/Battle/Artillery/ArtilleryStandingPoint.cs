using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Battle.Artillery
{
    public class ArtilleryStandingPoint : StandingPoint
    {
        public override bool IsDisabledForAgent(Agent agent)
        {
            return !agent.HasAttribute("ArtilleryCrew") || base.IsDisabledForAgent(agent);
        }
    }

    public class AmmoPickUpStandingPoint : StandingPointWithWeaponRequirement
    {
        public override bool IsDisabledForAgent(Agent agent)
        {
            return !agent.HasAttribute("ArtilleryCrew") || base.IsDisabledForAgent(agent);
        }
    }
}
