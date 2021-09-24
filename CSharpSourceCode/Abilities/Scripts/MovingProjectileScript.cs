using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class MovingProjectileScript : AbilityScript
    {
        override protected bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius;
            var closestAgent = Mission.Current.RayCastForClosestAgent(_previousFrameOrigin, GameEntity.GetGlobalFrame().origin, out float _, _casterAgent.Index, collisionRadius);
            if (_casterAgent.HasMount)
            {
                return closestAgent != null && closestAgent.Index != _casterAgent.MountAgent.Index;
            }
            else
            {
                return closestAgent != null;
            }
        }
    }
}