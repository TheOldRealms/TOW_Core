using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class MovingProjectileScript : AbilityScript
    {
        override protected bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius;
            var closestAgent = Mission.Current.RayCastForClosestAgent(_previousFrameOrigin, GameEntity.GetGlobalFrame().origin, out float _, _casterAgent.Index, collisionRadius);
            if (closestAgent != null)
            {
                return closestAgent.Index != _casterAgent.MountAgent?.Index;
            }
            return false;
        }
    }
}