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

        public override void Show()
        {
            base.Show();
            _cachedTarget = null;
        }

        public override void Hide()
        {
            base.Hide();
            UnlockTarget();
        }

        private void FindTarget()
        {
            Vec3 sourcePoint = Vec3.Zero;
            Vec3 targetPoint = Vec3.Zero;
            _missionScreen.ScreenPointToWorldRay(Input.MousePositionRanged, out sourcePoint, out targetPoint);
            if (!_mission.CameraIsFirstPerson)
            {
                _missionScreen.GetProjectedMousePositionOnGround(out targetPoint, out _, true);
            }
            float collisionDistance;
            Agent newTarget = _mission.RayCastForClosestAgent(sourcePoint, targetPoint, out collisionDistance);
            if (newTarget == null)
            {
                UnlockTarget();
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
                if (newTarget != _cachedTarget)
                {
                    UnlockTarget();
                }

                LockTarget(newTarget);
            }
            else
            {
                UnlockTarget();
            }
        }

        private void LockTarget(Agent newTarget)
        {
            _cachedTarget = newTarget;
            if (newTarget.IsEnemyOf(_caster))
            {
                _cachedTarget.AgentVisuals.SetContourColor(enemyColor);
            }
            else
            {
                _cachedTarget.AgentVisuals.SetContourColor(friendColor);
            }
            _isTargetLocked = true;
        }

        public void UnlockTarget()
        {
            _cachedTarget?.AgentVisuals?.SetContourColor(colorLess);
            _isTargetLocked = false;
        }


        public Agent CachedTarget
        {
            get => _cachedTarget;
        }

        public bool IsTargetLocked
        {
            get => _isTargetLocked;
        }

        private Agent _cachedTarget;

        private bool _isTargetLocked;
    }
}
