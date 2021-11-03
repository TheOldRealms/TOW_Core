using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.TriggeredEffect;

namespace TOW_Core.Abilities.Scripts
{
    public abstract class AbilityScript : ScriptComponentBehaviour
    {
        protected Ability _ability;
        private int _soundIndex;
        private SoundEvent _sound;
        protected Agent _casterAgent;
        protected float _abilityLife = -1;
        protected bool _isFading;
        protected float _timeSinceLastTick = 0;
        private bool _hasCollided;
        private bool _hasTickedOnce;
        protected bool _hasTriggered;
        private bool _soundStarted;
        protected Vec3 _previousFrameOrigin;
        private float _minArmingTimeForCollision = 0.1f;
        private bool _canCollide;

        internal void SetAgent(Agent agent)
        {
            _casterAgent = agent;
        }
        protected override bool MovesEntity() => true;
        protected virtual bool ShouldMove()
        {
            return _ability.Template.AbilityEffectType != AbilityEffectType.TargetedStaticAOE &&
                   _ability.Template.AbilityEffectType != AbilityEffectType.CenteredStaticAOE &&
                   _ability.Template.AbilityEffectType != AbilityEffectType.Summoning;
        }

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
            if (_ability.Template.SoundEffectToPlay != "none")
            {
                _soundIndex = SoundEvent.GetEventIdFromString(_ability.Template.SoundEffectToPlay);
                _sound = SoundEvent.CreateEvent(_soundIndex, Scene);
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (_ability == null) return;
            if (_isFading) return;
            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);

            var frame = GameEntity.GetGlobalFrame();
            UpdateSound(frame.origin);

            if (_ability.Template.TriggerType == TriggerType.OnCollision && CollidedWithAgent())
            {
                HandleCollision(frame.origin, frame.origin.NormalizedCopy());
            }
            if (_ability.Template.TriggerType == TriggerType.EveryTick && _timeSinceLastTick > _ability.Template.TickInterval)
            {
                _timeSinceLastTick = 0;
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
            }
            else if (_ability.Template.TriggerType == TriggerType.TickOnce && _abilityLife > _ability.Template.TickInterval && !_hasTriggered)
            {
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
                _hasTriggered = true;
            }
            _hasTickedOnce = true;
            if (ShouldMove())
            {
                UpdatePosition(frame, dt);
            }
        }

        protected void UpdatePosition(MatrixFrame frame, float dt)
        {
            var newframe = GetNextFrame(frame, dt);
            GameEntity.SetGlobalFrame(newframe);
            if (GameEntity.GetBodyShape() != null) GameEntity.GetBodyShape().ManualInvalidate();
        }

        protected virtual MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            _previousFrameOrigin = oldFrame.origin;
            return oldFrame.Advance(_ability.Template.BaseMovementSpeed * dt);
        }

        protected void UpdateLifeTime(float dt)
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
            if (_abilityLife > _minArmingTimeForCollision)
            {
                _canCollide = true;
            }
        }

        protected void UpdateSound(Vec3 position)
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
            return Mission.Current
                .GetAgentsInRange(GameEntity.GetGlobalFrame().origin.AsVec2, collisionRadius, true)
                .Any(agent => agent != _casterAgent && Math.Abs(GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < collisionRadius);
        }

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            if (_ability.Template.TriggerType == TriggerType.OnCollision && _canCollide)
            {
                HandleCollision(contact.ContactPair0.Contact0.Position, contact.ContactPair0.Contact0.Normal);
            }
        }

        protected virtual void HandleCollision(Vec3 position, Vec3 normal)
        {
            if (!_hasTickedOnce) return;
            if (!_hasCollided && position.IsValid && position.IsNonZero)
            {
                GameEntity.FadeOut(0.05f, true);
                _isFading = true;
                TriggerEffect(position, normal);
                _hasCollided = true;
            }
        }

        protected void TriggerEffect(Vec3 position, Vec3 normal)
        {
            var effect = TriggeredEffectManager.CreateNew(_ability?.Template.TriggeredEffectID);
            if (effect != null)
            {
                if (_ability.Template.CrosshairType == CrosshairType.Targeted)
                {
                    var crosshair = (TargetedCrosshair)_ability.Crosshair;
                    var target = crosshair.LastTargetIndex != -1 ? Mission.Current.Agents.FirstOrDefault(a => a.Index == crosshair.LastTargetIndex) : null;
                    if (target != null)
                    {
                        effect.Trigger(position, normal, _casterAgent, new List<Agent>(1) { target });
                    }
                    return;
                }
                effect.Trigger(position, normal, _casterAgent);
            }
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _sound?.Release();
            _sound = null;
            _ability = null;
            _casterAgent = null;
        }
    }
}
