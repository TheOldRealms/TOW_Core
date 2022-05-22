using SandBox;
using SandBox.Source.Objects.SettlementObjects;
using System;
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
        Chair _chair;
        StandingPoint _standingPoint;

        public override void Initialize(Ability ability)
        {
            base.Initialize(ability);
            var sphere = GameEntity.Instantiate(Mission.Current.Scene, "bd_chair_c_left", true);
            sphere.BodyFlag &= BodyFlags.DontCollideWithCamera & BodyFlags.DoNotCollideWithRaycast;
            GameEntity.AddChild(sphere);

            // As long as the player is locked in interaction with an object,
            // Agent.TeleportToPosition doesn't lock to the navmesh so we can
            // teleport to any position within 3D space.
            _chair = sphere.GetScriptComponents<Chair>().ElementAtOrValue(0, null);
            _standingPoint = _chair.GameEntity
                .GetChild(0).GetScriptComponents<StandingPoint>().ElementAtOrValue(0, null);

            DisbindKeyBindings();
        }

        public override void SetAgent(Agent agent)
        {
            base.SetAgent(agent);
            //agent.Disappear();
            agent.SetInvulnerable(true);
            var frame = _casterAgent.Frame.Elevate(1f);

            // Line up the chair and standing point so that the user is able to 
            // quickly take a seat.
            GameEntity.SetGlobalFrame(frame);
            _chair.GameEntity.SetGlobalFrame(frame);
            _standingPoint.GameEntity.SetGlobalFrame(frame);

            _casterAgent.SetUsedGameObjectForClient(_standingPoint);
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
            if (_casterAgent == null || !_chair.IsAgentFullySitting(_casterAgent))
            {
                // Teleport the chair to the agent's feet to allow for fast seating.
                var frame = GameEntity.GetGlobalFrame();
                frame.origin = new Vec3(_casterAgent.Position.x, _casterAgent.Position.y,
                    _casterAgent.GetEyeGlobalPosition().z - _casterAgent.GetEyeGlobalHeight());
                GameEntity.SetGlobalFrame(frame);

                // Disable moving the chair while the agent isn't "seated" or else
                // It could cause problems where the agent isn't seated immediately
                return;
            }

            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);

            if (_casterAgent != null && _casterAgent.Health > 0)
            {
                UpdateSound(_casterAgent.Position);
                TeleportAgentToFlyingChair(_casterAgent, _standingPoint.GameEntity);
                
                if (Input.IsKeyDown(InputKey.W) || Input.IsKeyPressed(InputKey.W))
                {
                    Fly(dt);

                    // Collision ray casting needs to be focused on CameraFrame.forward since movement is based on CameraFrame
                    /*var dist = GetDistance();
                    if (dist > 3 || dist.Equals(float.NaN))
                    {
                        Fly(dt);
                    }*/
                }
            }

            /*var agents = Mission.Current.GetAgentsInRange(_casterAgent.Position.AsVec2, 2);
            foreach (Agent agent in agents)
            {
                if (agent != _casterAgent && MathF.Abs(_casterAgent.Position.Z - agent.Position.Z) < 1)
                {
                    Vec3 pos = agent.Position - _casterAgent.Position;
                    pos.Normalize();
                    pos.z = 0;
                    agent.TeleportToPosition(agent.Position + pos);
                }
            }*/
        }

        private float GetDistance()
        {
            float num;
            GameEntity collidedEntity;

            Vec3 pos = Mission.Current.GetCameraFrame().Advance(3).origin;
            Mission.Current.Scene.RayCastForClosestEntityOrTerrain(_casterAgent.Position, pos, out num, out collidedEntity, 0.01f);
            return num;
        }
        
        private void Fly(float dt)
        {
            MatrixFrame frame = GameEntity.GetGlobalFrame();
            
            // Agent.LookFrame doesn't work because agents are stuck looking forward
            // while seated.
            //frame.rotation = _casterAgent.LookFrame.rotation;

            frame.rotation = Mat3.CreateMat3WithForward(Mission.Current.GetCameraFrame().rotation.u * -1);
            frame.Advance(_speed * dt);
            GameEntity.SetGlobalFrame(frame);
            _casterAgent.TeleportToPosition(frame.origin);
        }

        public override void Stop()
        {
            base.Stop();
            BindKeyBindings();
            _casterAgent.StopUsingGameObject();
            _casterAgent.SetActionChannel(0, ActionIndexCache.act_none, true);
            _casterAgent.Appear();
            _casterAgent.SetInvulnerable(false);
        }

        // Setting up infrastructure in case we want to add offsets for whatever reason
        private void TeleportAgentToFlyingChair(Agent agent, GameEntity entity)
        {
            agent.TeleportToPosition(
                new Vec3(
                    entity.GlobalPosition.x, 
                    entity.GlobalPosition.y, 
                    entity.GlobalPosition.z));
        }

        public bool IsFadinOut { get => _isFading; }

        private float _speed = 10f;
        private InputKey[] _axisKeys = new InputKey[4];
        private GameKeyContext _keyContext = HotKeyManager.GetCategory("CombatHotKeyCategory");
    }
}