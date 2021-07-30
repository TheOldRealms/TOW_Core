using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class MovingProjectileScript : AbilityScript
    {
        override protected bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius;
            var rayCastForClosestAgent = Mission.Current.RayCastForClosestAgent(_previousFrameOrigin, GameEntity.GetGlobalFrame().origin, out float _, _casterAgent.Index, collisionRadius);
            return rayCastForClosestAgent != null;
        }
    }
}