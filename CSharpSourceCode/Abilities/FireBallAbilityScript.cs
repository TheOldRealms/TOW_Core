using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Extensions;
using TOW_Core.Battle;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    public class FireBallAbilityScript : ScriptComponentBehaviour
    {
        private Agent _casterAgent;
        private float _radius = 5f;
        private int _minDamage = 20;
        private int _maxDamage = 30;
        private bool _hasCollided;
        private float _speed = 35f;
        private FireBallAbility _ability;
        private float _abilitylife = -1f;
        private float _collisionRadius = 2f;
        private bool _isFading;
        private int _movingSoundindex;
        private int _explosionSoundindex;
        private SoundEvent _movingSound;
        private SoundEvent _explosionSound;
        private float _elevationSpeed = 5f;

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            //clean up
            if (_movingSound != null) _movingSound.Release();
            if (_explosionSound != null) _explosionSound.Release();
            _casterAgent = null;
            _ability = null;
            _movingSound = null;
        }

        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        protected override bool MovesEntity() => true;
        public void SetAgent(Agent agent) => _casterAgent = agent;
        public void SetAbility(FireBallAbility fireBallAbility) => _ability = fireBallAbility;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
            _movingSoundindex = SoundEvent.GetEventIdFromString("fireball");
            _explosionSoundindex = SoundEvent.GetEventIdFromString("fireball_explosion");
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            var frame = base.GameEntity.GetGlobalFrame();

            if (CollidedWithAgent())
            {
                HandleCollision(frame.origin, frame.origin.NormalizedCopy());
            }
            
            var newframe = frame.Advance(_speed * dt);
            _elevationSpeed = _elevationSpeed - 8f * dt;
            base.GameEntity.SetGlobalFrame(newframe.Elevate(_elevationSpeed * dt));

            base.GameEntity.GetBodyShape().ManualInvalidate();
            if (_abilitylife < 0)
            {
                _abilitylife = 0;
            }
            else
            {
                _abilitylife += dt;
            }

            if (_ability != null)
            {
                if (_abilitylife > _ability.MaxDuration && !_isFading)
                {
                    base.GameEntity.FadeOut(0.1f, true);
                    _isFading = true;
                }
            }

            if (_movingSound == null)
            {
                _movingSound = SoundEvent.CreateEvent(_movingSoundindex, Scene);
                _movingSound.SetPosition(newframe.origin);
                _movingSound.Play();
            }

            _movingSound.SetPosition(newframe.origin);
        }

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            HandleCollision(contact.ContactPair0.Contact0.Position, contact.ContactPair0.Contact0.Normal);
        }

        private void HandleCollision(Vec3 collisionPoint, Vec3 collisionNormal)
        {
            if (!_hasCollided)
            {
                //start fading out for the projectile
                GameEntity.FadeOut(0.05f, false);

                //apply AOE damage
                TOWBattleUtilities.DamageAgentsInArea(collisionPoint.AsVec2, _radius, _minDamage, _maxDamage, _casterAgent, true);
                TOWBattleUtilities.ApplyStatusEffectToAgentsInArea(collisionPoint.AsVec2, _radius, "fireball_dot", _casterAgent, true);

                //create visual explosion
                var explosion = GameEntity.CreateEmpty(Scene);
                MatrixFrame frame = MatrixFrame.Identity;
                ParticleSystem.CreateParticleSystemAttachedToEntity("psys_burning_projectile_default_coll", explosion, ref frame);
                var globalFrame = new MatrixFrame(Mat3.CreateMat3WithForward(in collisionNormal), collisionPoint);
                explosion.SetGlobalFrame(globalFrame);
                explosion.FadeOut(3, true);

                //play explosion sound
                if (_explosionSound == null) _explosionSound = SoundEvent.CreateEvent(_explosionSoundindex, Scene);
                _explosionSound.SetPosition(globalFrame.origin);
                _explosionSound.Play();

                //Mission.Current.MakeSound(_explosionSoundindex, globalFrame.origin, false, true, -1, -1);
                //set flag so we dont run this again (there can be multiple collisions, because fadeout is not instant)
                _hasCollided = true;
                if (_movingSound != null) _movingSound.Release();
            }
        }

        private bool CollidedWithAgent()
        {
            return Mission.Current.GetAgentsInRange(GameEntity.GetGlobalFrame().origin.AsVec2, _collisionRadius)
                .Where(agent => agent != Agent.Main && Math.Abs(GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < _collisionRadius)
                .Any();
        }
    }
}