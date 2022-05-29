using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class GrenadeScript : BlackPowderWeaponScript
    {
        protected override void OnInit()
        {
            SetScriptComponentToTick(GetTickRequirement());
        }

        protected override void OnTick(float dt)
        {
        }

        protected override void OnRemoved(int removeReason)
        {
            Explode();
        }

        private void Explode()
        {
            _explosion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, _shooterAgent);
            GameEntity.FadeOut(0.5f, true);
        }
    }
}
