using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Crosshairs
{
    public class DirectionalAOECrosshair : AbilityCrosshair
    {
        public DirectionalAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            this._caster = caster;
            _crosshair = GameEntity.Instantiate(_mission.Scene, "custom_marker", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            IsVisible = false;
        }
        public override void Tick()
        {
            UpdateFrame();
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

                if (_currentDistance < template.MinDistance)
                {
                    _position = _frame.Advance(template.MinDistance).origin;
                    _position.z = _currentHeight;
                }
                else if (_currentDistance > template.MaxDistance)
                {
                    _position = _caster.LookFrame.Advance(template.MaxDistance).origin;
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
