using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class GrenadeScript : ScriptComponentBehavior
    {
        private bool _isExploded;
        private object _locker = new object();
        private Agent _shooter;
        private TriggeredEffect _explosion;


        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            Explode();
        }

        private void Explode()
        {
            lock (_locker)
            {
                if (!_isExploded)
                {
                    _isExploded = true;
                    _explosion.Trigger(GameEntity.GlobalPosition, Vec3.Zero, _shooter);
                    GameEntity.FadeOut(0.5f, true);
;                }
            }
        }

        public void SetShooterAgent(Agent shooter)
        {
            _shooter = shooter;
        }

        public void SetTriggeredEffect(TriggeredEffect effect)
        {
            _explosion = effect;
        }
    }
}
