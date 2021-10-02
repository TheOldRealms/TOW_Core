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
                Move(GameEntity.GetGlobalFrame());
                _abilityLife = 0;
                _casterAgent.SetInvulnerable(true);
                if (shouldDisappear) _casterAgent.Disappear();
                _hasTriggered = true;
            }
            else
            {
                _abilityLife += dt;
                var frame = GetNextFrame(GameEntity.GetGlobalFrame());
                if (_abilityLife > _ability.Template.Duration && !_isFading)
                {
                    GameEntity.FadeOut(0.05f, true);
                    _isFading = true;
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
            var heightAtPosition = Mission.Current.Scene.GetGroundHeightAtPosition(frame.origin);
            frame.origin.z = heightAtPosition + base._ability.Template.Radius / 2;
            return frame;
        }
    }
}
