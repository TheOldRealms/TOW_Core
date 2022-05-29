using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class MissileScript : AbilityScript
    {
        override protected bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius;
            var index = _casterAgent.Health <= 0 ? -1 : _casterAgent.Index;
            var closestAgent = Mission.Current.RayCastForClosestAgent(_previousFrameOrigin, GameEntity.GetGlobalFrame().origin, out float _, index, collisionRadius);
            
            return closestAgent != null && closestAgent.Index != _casterAgent.MountAgent?.Index;
        }
    }
}