using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.Scripts
{
    public class ShadowStepScript : AbilityScript
    {
        protected override void OnTick(float dt)
        {
            if (_isFading)
            {
                return;
            }
            if (!_hasTriggered)
            {
                _hasTriggered = true;
                _abilityLife = 0;
                _casterAgent.SetInvulnerable(true);
                _casterAgent.Disappear();
                _speed = _ability.Template.BaseMovementSpeed / 2;

                var frame = _casterAgent.Frame.Elevate(1);
                var sphere = GameEntity.Instantiate(Scene, "magic_sphere", MatrixFrame.Identity);
                sphere.AddBodyFlags(BodyFlags.DontCollideWithCamera);
                sphere.AddBodyFlags(BodyFlags.Barrier3D);
                sphere.EntityVisibilityFlags = EntityVisibilityFlags.VisibleOnlyForEnvmap;
                GameEntity.SetGlobalFrame(frame);
                GameEntity.AddChild(sphere);
            }
            else
            {
                _abilityLife += dt;
                ChangePosition();
                if (_abilityLife > _ability.Template.Duration && !_isFading)
                {
                    Stop();
                }
            }
        }

        private void ChangePosition()
        {
            MatrixFrame frame = GameEntity.GetFrame();
            frame.rotation = Agent.Main.LookRotation;
            if (Input.IsKeyPressed(InputKey.W) || Input.IsKeyDown(InputKey.W))
            {
                frame.Advance(_speed);
            }
            if (Input.IsKeyPressed(InputKey.S) || Input.IsKeyDown(InputKey.S))
            {
                frame.Advance(-_speed);
            }
            if (Input.IsKeyPressed(InputKey.A) || Input.IsKeyDown(InputKey.A))
            {
                frame.Strafe(-_speed);
            }
            if (Input.IsKeyPressed(InputKey.D) || Input.IsKeyDown(InputKey.D))
            {
                frame.Strafe(_speed);
            }
            GameEntity.SetGlobalFrame(frame);
        }

        public void Stop()
        {
            GameEntity.FadeOut(0.05f, true);
            _casterAgent.Appear();
            _casterAgent.SetInvulnerable(false);
            _isFading = true;
        }


        public bool IsFadinOut { get => _isFading; }

        private float _speed;
    }
}
