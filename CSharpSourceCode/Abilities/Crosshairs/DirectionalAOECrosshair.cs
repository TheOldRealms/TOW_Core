using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class DirectionalAOECrosshair : AbilityCrosshair
    {
        public DirectionalAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            _caster = caster;
            _crosshair = GameEntity.CreateEmpty(_mission.Scene, false);
            GameEntity decal = GameEntity.Instantiate(_mission.Scene, "ground_empire_wind_decal", false);

            MatrixFrame frame = decal.GetFrame();
            frame.rotation.RotateAboutUp(180f.ToRadians());
            frame.Scale(new Vec3(template.Radius * 5, template.Radius * 5, 1, -1));
            frame.Advance(-0.8f);
            frame.Strafe(0.025f);
            decal.SetFrame(ref frame);
            _crosshair.AddChild(decal);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            InitializeColors();
            _currentIndex = 0;
            _crosshair.SetFactorColor(_colors[_currentIndex]);
            AddLight();
            IsVisible = false;
        }

        public override void Tick()
        {
            UpdateFrame();
            ChangeColor();
        }

        private void UpdateFrame()
        {
            if (_caster != null)
            {
                _missionScreen.GetProjectedMousePositionOnGround(out _position, out _normal, true);
                _currentHeight = _mission.Scene.GetGroundHeightAtPosition(Position);
                _currentDistance = _caster.Position.Distance(_position);
                _frame = _caster.LookFrame;
                _frame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();

                if (_currentDistance < _template.MinDistance)
                {
                    _position = _frame.Advance(_template.MinDistance).origin;
                    _position.z = _currentHeight;
                }
                else if (_currentDistance > _template.MaxDistance)
                {
                    _position = _caster.LookFrame.Advance(_template.MaxDistance).origin;
                    _position.z = _currentHeight;
                }

                _frame.origin = _position;
                _crosshair.SetGlobalFrame(_frame);
            }
        }

        private float _currentHeight;

        private float _currentDistance;

        private Vec3 _position;

        private Vec3 _normal;

        private MatrixFrame _frame;

        private Agent _caster;
    }
}
