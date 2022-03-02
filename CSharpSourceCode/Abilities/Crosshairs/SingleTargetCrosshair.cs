using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SingleTargetCrosshair : MissileCrosshair
    {
        public SingleTargetCrosshair(AbilityTemplate template) : base(template)
        {
        }

        public override void Tick()
        {
            FindTarget();
        }

        public override void Hide()
        {
            base.Hide();
            _target?.AgentVisuals.SetContourColor(colorLess);
        }

        private void FindTarget()
        {
            base.Tick();
            Vec3 sourcePoint = Vec3.Zero;
            Vec3 targetPoint = Vec3.Zero;
            if (_mission.CameraIsFirstPerson)
            {
                _missionScreen.ScreenPointToWorldRay(Input.MousePositionRanged, out sourcePoint, out targetPoint);
            }
            else
            {
                sourcePoint = _missionScreen.CombatCamera.Position;
                targetPoint = _caster.Position + _caster.LookDirection.NormalizedCopy() * _template.MaxDistance;
            }
            float collisionDistance;
            Agent newTarget = _mission.RayCastForClosestAgent(sourcePoint, targetPoint, out collisionDistance);
            if (newTarget == null)
            {
                RemoveTarget();
                return;
            }
            if (newTarget.IsMount && newTarget.RiderAgent != null)
            {
                newTarget = newTarget.RiderAgent;
            }
            var targetType = _template.AbilityTargetType;
            bool isTargetMatching = collisionDistance <= _template.MaxDistance &&
                                    (targetType == AbilityTargetType.SingleEnemy && newTarget.IsEnemyOf(_caster)) ||
                                    (targetType == AbilityTargetType.SingleAlly && !newTarget.IsEnemyOf(_caster));
            if (isTargetMatching)
            {
                if (newTarget != _target)
                {
                    RemoveTarget();
                }

                SetTarget(newTarget);
            }
            else
            {
                RemoveTarget();
            }
        }

        private void SetTarget(Agent newTarget)
        {
            _target = newTarget;
            if (newTarget.IsEnemyOf(_caster))
            {
                _target.AgentVisuals.SetContourColor(enemyColor);
            }
            else
            {
                _target.AgentVisuals.SetContourColor(friendColor);
            }
        }

        private void RemoveTarget()
        {
            if (_target != null)
            {
                _target.AgentVisuals?.SetContourColor(colorLess);
                _target = null;
            }
        }


        public Agent Target
        {
            get => _target;
        }

        private Agent _target;
    }
}
