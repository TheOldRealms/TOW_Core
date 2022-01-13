using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities.Scripts
{
    public class ShadowStepScript : AbilityScript
    {
        public override void Initialize(Ability ability)
        {
            base.Initialize(ability);
            var sphere = GameEntity.Instantiate(Mission.Current.Scene, "flying_sphere", true);
            sphere.BodyFlag = BodyFlags.Barrier3D;
            sphere.BodyFlag |= BodyFlags.DontCollideWithCamera;
            sphere.EntityVisibilityFlags |= EntityVisibilityFlags.VisibleOnlyForEnvmap;
            GameEntity.AddChild(sphere);
            DisbindKeyBindings();
        }

        internal override void SetAgent(Agent agent)
        {
            base.SetAgent(agent);
            agent.Disappear();
            agent.SetInvulnerable(true);
            var frame = _casterAgent.Frame.Elevate(1f);
            GameEntity.SetGlobalFrame(frame);
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
            if (_ability == null) return;
            if (_isFading) return;
            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);

            if (_casterAgent != null && _casterAgent.Health > 0)
            {
                UpdateSound(_casterAgent.Position);
                var dist = GetDistance();
                if (Input.IsKeyDown(InputKey.W) || Input.IsKeyPressed(InputKey.W))
                {
                    if (dist > 3 || dist.Equals(float.NaN))
                    {
                        Fly();
                    }
                }
            }
        }

        private float GetDistance()
        {
            float num;
            Vec3 pos = _casterAgent.LookFrame.Advance(3).origin;
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(_casterAgent.Position + new Vec3(0f, 0f, _casterAgent.GetEyeGlobalHeight(), -1f), pos, out num);
            return num;
        }
        
        private void Fly()
        {
            MatrixFrame frame = GameEntity.GetGlobalFrame();
            frame.rotation = _casterAgent.LookRotation;
            frame.Advance(_speed);
            GameEntity.SetGlobalFrame(frame);
        }

        public override void Stop()
        {
            base.Stop();
            BindKeyBindings();
            _casterAgent.Appear();
            _casterAgent.SetInvulnerable(false);
        }


        public bool IsFadinOut { get => _isFading; }

        private float _speed = 0.3f;
        private InputKey[] _axisKeys = new InputKey[4];
        private GameKeyContext _keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
    }
}