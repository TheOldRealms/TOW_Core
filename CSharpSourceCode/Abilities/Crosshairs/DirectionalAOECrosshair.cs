using System.IO;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities.Crosshairs
{
    public class DirectionalAOECrosshair : AbilityCrosshair
    {
        public DirectionalAOECrosshair(AbilityTemplate template, Agent caster) : base(template)
        {
            this._caster = caster;
            _crosshair = GameEntity.Instantiate(_mission.Scene, "custom_marker", false);
            _crosshair.EntityFlags |= EntityFlags.NotAffectedBySeason;
            //var frame = _crosshair.GetFrame();
            //frame.Scale(new Vec3(3, 10, 1, -1));
            //_crosshair.SetFrame(ref _frame);
            IsVisible = false;
        }
        public override void Tick()
        {
            //Test();
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (_caster != null)
            {
                _missionScreen.GetProjectedMousePositionOnGround(out _position, out _normal, true);
                _currentHeight = _mission.Scene.GetGroundHeightAtPosition(Position);
                _currentDistance = _caster.Position.Distance(_position);
                _frame = _caster.AgentVisuals.GetGlobalFrame();
                if (_currentDistance < template.MinDistance)
                {
                    _position = _frame.Advance(2).origin;
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

        private void Test()
        {
            if (!isBound)
            {
                if (_caster.AgentVisuals != null)
                {
                    isBound = true;
                    _frame = MatrixFrame.Identity;
                    _frame.Advance(3);
                    Frame = _frame;
                    _caster.AgentVisuals.AddChildEntity(_crosshair);
                }
            }
        }


        private bool isBound;
        private float _currentHeight;
        private float _currentDistance;
        private Vec3 _position;
        private Vec3 _normal;
        private MatrixFrame _frame;
        private Agent _caster;
    }
}
