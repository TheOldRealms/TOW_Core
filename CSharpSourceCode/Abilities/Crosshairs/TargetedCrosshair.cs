using System;
using TaleWorlds.Library;
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
            FindAim();
        }

        public override void Hide()
        {
            base.Hide();
            RemoveAim();
        }

        private void FindAim()
        {
            Vec3 position;
            Vec3 normal;
            _missionScreen.GetProjectedMousePositionOnGround(out position, out normal);

            float distance;
            var target = Mission.Current.RayCastForClosestAgent(_caster.GetEyeGlobalPosition(), position, out distance, _caster.Index, 0.1f);
            var targetType = _template.AbilityTargetType;
            bool isAimMatching = target != null &&
                                 target.IsHuman &&
                                 (targetType == AbilityTargetType.All ||
                                 (targetType == AbilityTargetType.Enemies && target.IsEnemyOf(_caster)) ||
                                 (targetType == AbilityTargetType.Allies && !target.IsEnemyOf(_caster)));
            if (isAimMatching)
            {
                if (target != _target)
                {
                    RemoveAim();
                }
                SetTarget(target);
            }
            else
            {
                RemoveAim();
            }
        }

        private void SetTarget(Agent newAim)
        {
            _target = newAim;
            _lastTargetIndex = newAim.Index;
            if (newAim.IsEnemyOf(_caster))
            {
                _target.AgentVisuals.GetEntity().Root.SetContourColor(enemyColor);
            }
            else
            {
                _target.AgentVisuals.GetEntity().Root.SetContourColor(friendColor);
            }
        }

        private void RemoveAim()
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
