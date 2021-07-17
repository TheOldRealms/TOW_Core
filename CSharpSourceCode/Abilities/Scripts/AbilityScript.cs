using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.TriggeredEffect;

namespace TOW_Core.Abilities.Scripts
{
    public abstract class AbilityScript : ScriptComponentBehaviour
    {
        private Ability _ability;
        private int _soundIndex;
        private SoundEvent _sound;
        private Agent _casterAgent;
        private float _abilityLife = -1;
        private bool _isFading;
        private float _timeSinceLastTick = 0;
        private bool _hasCollided;

        internal void SetAgent(Agent agent)
        {
            _casterAgent = agent;
        }
        protected override bool MovesEntity() => true;
        protected virtual bool ShouldMove()
        {
            return _ability.Template.AbilityEffectType != (AbilityEffectType.TargetedStaticAOE | AbilityEffectType.CenteredStaticAOE);
        }

        protected override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            if (_sound != null) _sound.Release();
            _sound = null;
            _ability = null;
            _casterAgent = null;
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

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            if(_ability.Template.TriggerType == TriggerType.OnCollision)
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

        protected virtual MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            return oldFrame.Advance(_ability.Template.BaseMovementSpeed * dt);
        }


        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (_isFading) return;
            var frame = GameEntity.GetGlobalFrame();
            //handle agent collision
            if (_ability.Template.TriggerType == TriggerType.OnCollision && CollidedWithAgent())
            {
                HandleCollision(frame.origin, frame.origin.NormalizedCopy());
            }
            //move if needed
            if (ShouldMove())
            {
                var newframe = GetNextFrame(frame, dt);
                GameEntity.SetGlobalFrame(newframe);
                GameEntity.GetBodyShape().ManualInvalidate();
            }
            frame = GameEntity.GetGlobalFrame();
            //check lifetime/duration
            if (_abilityLife < 0)
            {
                _abilityLife = 0;
            }
            else
            {
                _abilityLife += dt;
            }

            if (_ability != null)
            {
                if (_abilityLife > _ability.Template.Duration && !_isFading)
                {
                    GameEntity.FadeOut(0.05f, true);
                    _isFading = true;
                }
            }
            //update sound if needed
            if (_sound != null)
            {
                _sound.SetPosition(GameEntity.GetGlobalFrame().origin);
                if(!_sound.IsPlaying() && _ability.Template.ShouldSoundLoopOverDuration) _sound.Play();
            }
            _timeSinceLastTick += dt;
            //trigger effects
            if (_ability.Template.TriggerType == TriggerType.EveryTick &&_timeSinceLastTick > _ability.Template.TickInterval)
            {
                _timeSinceLastTick = 0;
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
            }
            else if(_ability.Template.TriggerType == TriggerType.Delayed && _abilityLife > _ability.Template.TickInterval)
            {
                TriggerEffect(frame.origin, frame.origin.NormalizedCopy());
            }
        }

        private bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius + 0.5f;
            return Mission.Current.GetAgentsInRange(GameEntity.GetGlobalFrame().origin.AsVec2, collisionRadius)
                .Where(agent => agent != _casterAgent && Math.Abs(GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < collisionRadius)
                .Any();
        }

        private void TriggerEffect(Vec3 position, Vec3 normal)
        {
            var effect = TriggeredEffectManager.CreateNew(_ability?.Template.TriggeredEffectID);
            if(effect != null)
            {
                effect.Trigger(position, normal, _casterAgent);
            }
        }
    }
}
