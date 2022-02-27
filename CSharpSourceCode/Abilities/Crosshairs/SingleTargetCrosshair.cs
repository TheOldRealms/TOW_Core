﻿using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SingleTargetCrosshair : MissileCrosshair
    {
        public SingleTargetCrosshair(AbilityTemplate template, Agent caster) : base(template)
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
            _target?.AgentVisuals.SetContourColor(colorLess);
        }

        private void FindTarget()
        {
            Vec3 startRay, endRay;
            //startRay = _missionScreen.CombatCamera.Frame.origin;
            //endRay = _missionScreen.CombatCamera.Frame.Elevate(_template.MaxDistance).origin;
            _missionScreen.ScreenPointToWorldRay(new Vec2(960,540), out startRay, out endRay);
            var newTarget = Mission.Current.RayCastForClosestAgent(startRay, endRay, out _, _caster.Index, 0.01f);
            if (newTarget == null)
            {
                RemoveTarget();
                return;
            }
            if (newTarget.IsMount)
            {
                newTarget = newTarget.RiderAgent != null ? newTarget.RiderAgent : newTarget;
            }

            var targetType = _template.AbilityTargetType;
            bool isTargetMatching = (targetType == AbilityTargetType.EnemiesInAOE && newTarget.IsEnemyOf(_caster)) ||
                                    (targetType == AbilityTargetType.AlliesInAOE && !newTarget.IsEnemyOf(_caster));
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