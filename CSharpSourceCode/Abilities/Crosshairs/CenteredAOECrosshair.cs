using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class CenteredAOECrosshair : AOECrosshair
    {
        public CenteredAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            _caster = caster;
            _crosshair = GameEntity.Instantiate(Mission.Current.Scene, "targeting_rune_empire", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            MatrixFrame frame = _crosshair.GetFrame();
            frame.Scale(new Vec3(template.TargetCapturingRadius, template.TargetCapturingRadius, 1, -1));
            _crosshair.SetFrame(ref frame);
            InitializeColors();
            _currentIndex = 0;
            _crosshair.SetFactorColor(_colors[_currentIndex]);
            AddLight();
            IsVisible = false;
        }

        public override void Tick()
        {
            if (!_isBound)
            {
                if (_caster.AgentVisuals != null)
                {
                    _isBound = true;
                    _caster.AgentVisuals.AddChildEntity(_crosshair);
                }
            }
            HighlightNearbyAgents();
            Rotate();
            ChangeColor();
        }

        private bool _isBound;

        private Agent _caster;
    }
}
