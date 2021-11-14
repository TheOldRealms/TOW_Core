using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.Scripts
{
    public class ShadowStepScript : AbilityScript
    {
        public void Initialize(Agent agent)
        {
            _casterAgent = agent;
            _speed = _ability.Template.BaseMovementSpeed / 2;
            DisbindKeyBindings();
            var frame = _casterAgent.Frame.Elevate(1f);
            GameEntity.SetGlobalFrame(frame);
            GameEntity.BodyFlag = BodyFlags.Barrier3D;
            GameEntity.BodyFlag |= BodyFlags.DontCollideWithCamera;
            //Scale(0.7f);
            //void Scale(float scaling)
            //{
            //    var localFrame = GameEntity.GetFrame();
            //    localFrame.Scale(new Vec3(scaling, scaling, scaling));
            //    GameEntity.SetFrame(ref localFrame);
            //}
        }

        private void BindKeyBindings()
        {
            _keyContext.GetGameKey(0).KeyboardKey.ChangeKey(_axisKeys[0]);
            _keyContext.GetGameKey(1).KeyboardKey.ChangeKey(_axisKeys[1]);
            _keyContext.GetGameKey(2).KeyboardKey.ChangeKey(_axisKeys[2]);
            _keyContext.GetGameKey(3).KeyboardKey.ChangeKey(_axisKeys[3]);
        }

        private void DisbindKeyBindings()
        {
            for (int i = 0; i < 4; i++)
            {
                _axisKeys[i] = _keyContext.GetGameKey(i).KeyboardKey.InputKey;
                _keyContext.GetGameKey(i).KeyboardKey.ChangeKey(InputKey.Invalid);
            }
        }

        protected override void OnTick(float dt)
        {
            if (_casterAgent != null && _casterAgent.Health > 0)
            {
                var dist = GetDistance();
                if (Input.IsKeyDown(InputKey.W) || Input.IsKeyPressed(InputKey.W))
                {
                    if (dist > 3)
                    {
                        Fly();
                        TOWCommon.Say($"DIST {dist}");
                    }
                    else if (dist.Equals(float.NaN))
                    {
                        Fly();
                        TOWCommon.Say($"NaN {dist}");
                    }
                    else
                    {
                        TOWCommon.Say($"KEY {dist}");
                    }
                }
                if (Input.IsKeyPressed(InputKey.RightMouseButton))
                {
                    Stop();
                }

            }
            float GetDistance()
            {
                float num;
                Vec3 pos = _casterAgent.LookFrame.Advance(3).origin;
                Mission.Current.Scene.RayCastForClosestEntityOrTerrain(_casterAgent.Position + new Vec3(0f, 0f, _casterAgent.GetEyeGlobalHeight(), -1f), pos, out num);
                return num;
            }
            void Fly()
            {
                MatrixFrame frame = GameEntity.GetGlobalFrame();
                frame.rotation = _casterAgent.LookRotation;
                frame.Advance(_speed);
                GameEntity.SetGlobalFrame(frame);
            }
        }

        public void Stop()
        {
            GameEntity.FadeOut(0.05f, true);
            BindKeyBindings();
            base._casterAgent.Appear();
            base._casterAgent.SetInvulnerable(false);
            _isFading = true;
        }


        public bool IsFadinOut { get => _isFading; }

        private float _speed;
        private InputKey[] _axisKeys = new InputKey[4];
        private GameKeyContext _keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
    }
}