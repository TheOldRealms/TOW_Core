using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.Scripts
{
    public class SpecialMoveScript : AbilityScript
    {
        private bool shouldDisappear = true;
        private bool canChangeDirection = true;

        protected override void OnTick(float dt)
        {
            if (_isFading)
            {
                if (shouldDisappear) _casterAgent.Appear();
                _casterAgent.SetInvulnerable(false);
                return;
            }
            if (!_hasTriggered)
            {
                _hasTriggered = true;
                Move(GameEntity.GetGlobalFrame());
                _abilityLife = 0;
                _casterAgent.SetInvulnerable(true);
                if (shouldDisappear) _casterAgent.Disappear();
            }
            else
            {
                _abilityLife += dt;
                var frame = GetNextFrame(GameEntity.GetGlobalFrame());
                if (_abilityLife > _ability.Template.Duration && !_isFading)
                {
                    Stop();
                }
                Move(frame);
            }
        }

        private void Move(MatrixFrame frame)
        {
            GameEntity.SetGlobalFrame(frame);
            if (GameEntity.GetBodyShape() != null) GameEntity.GetBodyShape().ManualInvalidate();
            _casterAgent.TeleportToPosition(frame.origin);
            TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
            UpdateSound(frame.origin);
        }

        protected MatrixFrame GetNextFrame(MatrixFrame frame)
        {
            if (canChangeDirection)
            {
                frame.rotation = _casterAgent.LookRotation;
            }
            frame = frame.Advance(_ability.Template.BaseMovementSpeed);
            float height = 0;
            Mission.Current.Scene.GetHeightAtPoint(frame.origin.AsVec2, BodyFlags.None, ref height);
            frame.origin.z = height + base._ability.Template.Radius / 2;
            return frame;
        }

        public void Stop()
        {
            GameEntity.FadeOut(0.05f, true);
            _isFading = true;
        }


        public bool IsFadinOut { get => _isFading; }
    }
}
