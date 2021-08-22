using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class SummoningCrosshair : AbilityCrosshair
    {
        public SummoningCrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            _caster = caster;
            _crosshair = GameEntity.Instantiate(Mission.Current.Scene, "targeting_rune_empire", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            AddLight();
            InitializeColors();
            _currentIndex = 0;
            _crosshair.SetFactorColor(_colors[_currentIndex]);
            IsVisible = false;
        }

        public override void Tick()
        {
            UpdatePosition();
            Rotate();
            ChangeColor();
        }

        private void UpdatePosition()
        {
            if (_caster != null)
            {
                if (_missionScreen.GetProjectedMousePositionOnGround(out _position, out _normal, true))
                {
                    _currentDistance = _caster.Position.Distance(_position);
                    if (_currentDistance > _template.MaxDistance)
                    {
                        _position = _caster.LookFrame.Advance(_template.MaxDistance).origin;
                        _position.z = _mission.Scene.GetGroundHeightAtPosition(Position);
                    }
                    Position = _position;
                }
                else
                {
                    _position = _caster.LookFrame.Advance(_template.MaxDistance).origin;
                    _position.z = _mission.Scene.GetGroundHeightAtPosition(Position);
                    Position = _position; 
                }
            }
        }

        private float _currentDistance;
        private Vec3 _position;
        private Vec3 _normal;
        private Agent _caster;
    }
}
