using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace TOW_Core.Battle.TriggeredEffect.Scripts
{
    public class SmokingLimbScript : ScriptComponentBehavior
    {
        private float _liveTime = 0;
        private float _endTime = 0;
        private ParticleSystem _smokeParticles;
        private object _locker = new object();

        protected override void OnInit()
        {
            _endTime = MBRandom.RandomFloatRanged(12, 20);
            SetScriptComponentToTick(TickRequirement.Tick);
        }

        protected override void OnTick(float dt)
        {
            if (_liveTime < _endTime)
            {
                _liveTime += dt;
            }
            else
            {
                _smokeParticles?.SetEnable(false);
                SetScriptComponentToTick(TickRequirement.None);
            }
        }

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            if (_smokeParticles == null)
            {
                lock (_locker)
                {
                    var frame = GameEntity.GetFrame();
                    _smokeParticles = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_game_burning_jar_trail", GameEntity, ref frame);
                }
            }
        }
    }
}
