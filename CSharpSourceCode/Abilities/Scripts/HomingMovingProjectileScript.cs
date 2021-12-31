using TaleWorlds.Library;
using TOW_Core.Battle.AI.Decision;

namespace TOW_Core.Abilities.Scripts
{
    public class TargetedMovingProjectileScript : MovingProjectileScript
    {
        private float _proportional = 0.0f;
        private float _derivative = 0;

        private Target _target;
        private Vec3 _prevError;

        public void SetTarget(Target target)
        {
            _target = target;
            _prevError = Vec3.Zero;
        }

        public void SetSteeringGain(float proportional = 0f, float derivative = 0f)
        {
            _proportional = proportional;
            _derivative = derivative;
        }

        protected override void OnTick(float dt)
        {
            if (_target != null && (_target.Agent != null || _target.Formation.CountOfUnits > 0))
            {
                var globalFrame = GameEntity.GetGlobalFrame();
                var particleDirection = globalFrame.origin + globalFrame.rotation.f.NormalizedCopy();
                var error = _target.Position + new Vec3(0, 0, 2) - particleDirection;

                var correction = error * _proportional + (error - _prevError) * _derivative;
                var newDirection = particleDirection + correction * dt;
                var globalFrameRotation = Mat3.CreateMat3WithForward(newDirection - globalFrame.origin);

                globalFrame.rotation = globalFrameRotation;
                GameEntity.SetGlobalFrame(globalFrame);

                _prevError = error;
            }

            base.OnTick(dt);
        }
    }
}