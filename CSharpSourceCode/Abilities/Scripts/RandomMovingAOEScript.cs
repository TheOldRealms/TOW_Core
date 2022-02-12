using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Abilities.Scripts
{
    public class RandomMovingAOEScript : AbilityScript
    {
        private float _counter = 1f;
        private float _maxDeviation;
        private float _currentDeviation;
        private GameEntity _vortexPrefab;

        public override void Initialize(Ability ability)
        {
            base.Initialize(ability);
            _maxDeviation = _ability.Template.MaxRandomDeviation;
            var asd = GameEntity.GetChildren().ToList();
            _vortexPrefab = asd[0];
        }

        protected override void UpdatePosition(MatrixFrame frame, float dt)
        {
            var newFrame = GetNextFrame(frame, dt);
            GameEntity.SetGlobalFrame(newFrame);

            var vortexFrame = _vortexPrefab.GetFrame();
            vortexFrame.rotation.RotateAboutUp(1);
            _vortexPrefab.SetFrame(ref vortexFrame);

            if (GameEntity.GetBodyShape() != null) GameEntity.GetBodyShape().ManualInvalidate();
        }

        protected override MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            if (_counter >= 1)
            {
                _counter = 0;
                _currentDeviation = MBRandom.RandomFloatRanged(-_maxDeviation, _maxDeviation) * dt;
            }
            else if (_counter < 1)
            {
                _counter += dt;
            }
            oldFrame.rotation.RotateAboutUp(_currentDeviation);
            var distance = _ability.Template.BaseMovementSpeed * dt;
            oldFrame.Advance(distance);
            var heightAtPosition = Mission.Current.Scene.GetGroundHeightAtPosition(oldFrame.origin);
            oldFrame.origin.z = heightAtPosition + _ability.Template.Radius / 2;
            return oldFrame;
        }
    }
}
