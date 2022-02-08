using System;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class TargetedCrosshair : ProjectileCrosshair
    {
        public TargetedCrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            _caster = caster;
        }

        public override void Tick()
        {
            FindTarget();
        }

        public override void Hide()
        {
            base.Hide();
            _target?.AgentVisuals.GetEntity()?.Root.SetContourColor(colorLess);
        }

        private void FindTarget()
        {
            var endPoint = _caster.LookFrame.Elevate(_caster.GetEyeGlobalHeight()).Advance(_template.MaxDistance).origin;
            var newTarget = Mission.Current.RayCastForClosestAgent(_caster.GetEyeGlobalPosition(), endPoint, out _, _caster.Index, 0.01f);
            if (newTarget == null)
            {
                return;
            }
            if (newTarget.IsMount)
            {
                newTarget = newTarget.RiderAgent != null ? newTarget.RiderAgent : newTarget;
            }

            var targetType = _template.AbilityTargetType;
            bool isTargetMatching = targetType == AbilityTargetType.All ||
                                    (targetType == AbilityTargetType.Enemies && newTarget.IsEnemyOf(_caster)) ||
                                    (targetType == AbilityTargetType.Allies && !newTarget.IsEnemyOf(_caster));
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
            _lastTargetIndex = newTarget.Index;
            if (newTarget.IsEnemyOf(_caster))
            {
                _target.AgentVisuals.GetEntity().Root.SetContourColor(enemyColor);
            }
            else
            {
                _target.AgentVisuals.GetEntity().Root.SetContourColor(friendColor);
            }
        }

        private void RemoveTarget()
        {
            if (_target != null)
            {
                _target.AgentVisuals.GetEntity().Root.SetContourColor(colorLess);
                _target = null;
            }
        }


        public Int32 LastTargetIndex
        {
            get => _lastTargetIndex;
        }

        public Agent Target
        {
            get => _target;
        }

        private Int32 _lastTargetIndex = -1;

        private Agent _target;

        private Agent _caster;
    }
}
