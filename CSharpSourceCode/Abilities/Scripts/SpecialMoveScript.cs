using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.Scripts
{
    public class SpecialMoveScript : AbilityScript
    {
        protected override void OnTick(float dt)
        {
            if (_isFading)
            {
                if (_shouldDisappear)
                {
                    _casterAgent.Appear();
                }
                _casterAgent.SetInvulnerable(false);
                return;
            }
            if (!_hasTriggered)
            {
                _hasTriggered = true;

                var frame = _casterAgent.Frame.Elevate(1);
                var sphere = GameEntity.Instantiate(Scene, "magic_sphere_test", MatrixFrame.Identity);
                sphere.AddBodyFlags(BodyFlags.DontCollideWithCamera);
                sphere.AddBodyFlags(BodyFlags.Barrier3D);
                sphere.EntityVisibilityFlags = EntityVisibilityFlags.VisibleOnlyForEnvmap;
                GameEntity.SetGlobalFrame(frame);
                GameEntity.AddChild(sphere);
                _speed = _ability.Template.BaseMovementSpeed / 2;
                _casterAgent.SetSoundOcclusion(0);

                _abilityLife = 0;
                _casterAgent.SetInvulnerable(true);
                if (_shouldDisappear)
                {
                    _casterAgent.Disappear();
                }
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
            _isFading = true;
        }


        public bool IsFadinOut { get => _isFading; }

        private bool _shouldDisappear = true;
        private float _speed;
    }
}
