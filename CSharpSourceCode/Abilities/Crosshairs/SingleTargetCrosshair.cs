using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SingleTargetCrosshair : MissileCrosshair
    {
        public SingleTargetCrosshair(AbilityTemplate template, Agent caster) : base(template)
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
            Vec2 mousePositionRanged = Input.MousePositionRanged;
            Vec3 sourcePoint;
            Vec3 targetPoint;
            _missionScreen.ScreenPointToWorldRay(mousePositionRanged, out sourcePoint, out targetPoint);
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
            var targetType =  _template.AbilityTargetType;
            bool isTargetMatching = collisionDistance <= _template.MaxDistance && 
                                    (targetType == AbilityTargetType.All ||
                                    (targetType == AbilityTargetType.Enemies && newTarget.IsEnemyOf(_caster)) ||
                                    (targetType == AbilityTargetType.Allies && !newTarget.IsEnemyOf(_caster)));
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
                _target.AgentVisuals.SetContourColor(colorLess);
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
