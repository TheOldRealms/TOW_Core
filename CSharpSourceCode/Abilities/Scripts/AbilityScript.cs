using System;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.TriggeredEffect;

namespace TOW_Core.Abilities.Scripts
{
    public abstract class AbilityScript : ScriptComponentBehaviour
    {
        protected Ability _ability;
        private int _soundIndex;
        private SoundEvent _sound;
        protected Agent _casterAgent;
        private float _abilityLife = -1;
        private bool _isFading;
        private float _timeSinceLastTick = 0;
        private bool _hasCollided;
        private bool _hasTickedOnce;
        private bool _soundStarted;
        protected Vec3 _previousFrameOrigin;
        private MatrixFrame frame;

        internal void SetAgent(Agent agent)
        {
            _casterAgent = agent;
        }
        protected override bool MovesEntity() => true;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }

        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        public virtual void Initialize(Ability ability)
        {
            _ability = ability;
            frame = GameEntity.GetGlobalFrame();
            if (_ability.Template.SoundEffectToPlay != "none")
            {
                _soundIndex = SoundEvent.GetEventIdFromString(_ability.Template.SoundEffectToPlay);
                _sound = SoundEvent.CreateEvent(_soundIndex, Scene);
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (_isFading) return;
            _timeSinceLastTick += dt;

            if (_ability.IsDynamicAbility())
            {
                frame = UpdatePosition(dt);
            }
            if (_ability.Template.TriggerType == TriggerType.OnCollision && CollidedWithAgent())
            {
                HandleCollision(frame.origin, frame.origin.NormalizedCopy());
            }
            UpdateLifeTime(dt);
            UpdateSound(frame.origin);

            if (_ability.Template.TriggerType == TriggerType.EveryTick && _timeSinceLastTick > _ability.Template.TickInterval)
            {
                _timeSinceLastTick = 0;
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
                _hasTickedOnce = true;
            }
            else if (_ability.Template.TriggerType == TriggerType.TickOnce && _abilityLife > _ability.Template.TickInterval && !_hasTickedOnce)
            {
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
                _hasTickedOnce = true;
            }
        }

        private MatrixFrame UpdatePosition(float dt)
        {
            var newframe = GetNextFrame(GameEntity.GetGlobalFrame(), dt);
            GameEntity.SetGlobalFrame(newframe);
            GameEntity.GetBodyShape().ManualInvalidate();
            return newframe;
        }

        protected virtual MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            _previousFrameOrigin = oldFrame.origin;
            return oldFrame.Advance(_ability.Template.BaseMovementSpeed * dt);
        }

        private void UpdateLifeTime(float dt)
        {
            if (_abilityLife < 0) _abilityLife = 0;
            else _abilityLife += dt;
            if (_ability != null)
            {
                if (_abilityLife > _ability.Template.Duration && !_isFading)
                {
                    GameEntity.FadeOut(0.05f, true);
                    _isFading = true;
                }
            }
        }

        private void UpdateSound(Vec3 position)
        {
            if (_sound != null)
            {
                _sound.SetPosition(position);
                if (!_sound.IsPlaying())
                {
                    if (!_soundStarted)
                    {
                        _sound.Play();
                        _soundStarted = true;
                    }
                    else if (_ability.Template.ShouldSoundLoopOverDuration)
                    {
                        _sound.Play();
                    }
                }
            }
        }

        protected virtual bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius + 1;
            return Mission.Current.GetAgentsInRange(GameEntity.GetGlobalFrame().origin.AsVec2, collisionRadius, true)
                .Where(agent => agent != _casterAgent && Math.Abs(GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < collisionRadius)
                .Any();
        }

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            if (_ability.Template.TriggerType == TriggerType.OnCollision)
            {
                HandleCollision(contact.ContactPair0.Contact0.Position, contact.ContactPair0.Contact0.Normal);
            }
        }

        protected virtual void HandleCollision(Vec3 position, Vec3 normal)
        {
            if (!_hasCollided)
            {
                GameEntity.FadeOut(0.05f, true);
                _isFading = true;
                TriggerEffect(position, normal);
                _hasCollided = true;
            }
        }

        private void TriggerEffect(Vec3 position, Vec3 normal)
        {
            var effect = TriggeredEffectManager.CreateNew(_ability?.Template.TriggeredEffectID);
            if (effect != null)
            {
                effect.Trigger(position, normal, _casterAgent);
            }
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            if (_sound != null) _sound.Release();
            _sound = null;
            _ability = null;
            _casterAgent = null;
        }
    }
}
