using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using Messages.FromClient.ToLobbyServer;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
using TaleWorlds.Engine;
using System.Linq;
using TaleWorlds.Library;
using TOW_Core.Battle.Damage;
using TOW_Core.ObjectDataExtensions;
using static TaleWorlds.Core.ItemObject;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectComponent : AgentComponent
    {
        private float _updateFrequency = 1;
        private float _deltaSinceLastTick = (float)TOWMath.GetRandomDouble(0, 0.1);
        private Dictionary<StatusEffect, EffectData> _currentEffects;
        private EffectAggregate _effectAggregate;

        public StatusEffectComponent(Agent agent) : base(agent)
        {
            _currentEffects = new Dictionary<StatusEffect, EffectData>();
            _effectAggregate = new EffectAggregate();
        }

        public void RunStatusEffect(string id, Agent applierAgent)
        {
            if (Agent == null)
                return;

            StatusEffect effect = _currentEffects.Keys.Where(e => e.Template.Id.Equals(id)).FirstOrDefault();
            if (effect != null)
            {
                effect.CurrentDuration += effect.Template.BaseDuration;
            }
            else
            {
                effect = StatusEffectManager.GetStatusEffect(id);
                effect.CurrentDuration = effect.Template.BaseDuration;
                AddEffect(effect, applierAgent);
            }
        }


        public EffectAggregate GetEffectAggregate()
        {
            return _effectAggregate;
        }

        public void OnElapsed(float dt)
        {
            foreach (StatusEffect effect in _currentEffects.Keys)
            {
                effect.CurrentDuration--;
                if (effect.CurrentDuration <= 0)
                {
                    RemoveEffect(effect);
                    return;
                }
            }

            CalculateEffectAggregate();

            StatusEffect dotEffect = _currentEffects.Keys.Where(x => x.Template.Type == StatusEffectTemplate.EffectType.DamageOverTime).FirstOrDefault();
            EffectData data = null;
            if (dotEffect != null)
            {
                data = _currentEffects[dotEffect];
            }

            //Temporary method for applying effects from the aggregate. This needs to go to a damage manager/calculator which will use the 
            //aggregated information to determine how much damage to apply to the agent
            if (Agent.IsActive() && Agent != null && !Agent.IsFadingOut())
            {
                if (_effectAggregate.DamageOverTime > 0 && data != null)
                {
                    if (Agent.Health < _effectAggregate.DamageOverTime)
                    {
                        SpellBlowInfoManager.EnterSpellBlow(data.ApplierAgent.Index,"dot",DamageType.Magical);
                    }
                    
                    Agent.ApplyDamage(((int)_effectAggregate.DamageOverTime), data.ApplierAgent, false, false);
                }
                else if (_effectAggregate.HealthOverTime > 0)
                {
                    Agent.Heal((int)_effectAggregate.HealthOverTime);
                }
            }
        }

        private void CalculateEffectAggregate()
        {
            _effectAggregate = new EffectAggregate();
            foreach (var effect in _currentEffects.Keys)
            {
                _effectAggregate.AddEffect(effect);
            }
        }

        public void OnTick(float dt)
        {
            _deltaSinceLastTick += dt;
            if (_deltaSinceLastTick > _updateFrequency)
            {
                _deltaSinceLastTick = (float)TOWMath.GetRandomDouble(0, 0.1);
                OnElapsed(dt);
            }
        }

        private void RemoveEffect(StatusEffect effect)
        {
            EffectData data = _currentEffects[effect];

            data.ParticleEntities.ForEach(pe =>
            {
                pe.RemoveAllParticleSystems();
                pe = null;
            });

            _currentEffects.Remove(effect);
        }

        private void AddEffect(StatusEffect effect, Agent applierAgent)
        {
            List<GameEntity> childEntities;
            TOWParticleSystem.ApplyParticleToAgent(Agent, effect.Template.ParticleId, out childEntities, effect.Template.ParticleIntensity);

            EffectData data = new EffectData(effect, childEntities, applierAgent);
            data.ParticleEntities = childEntities;

            _currentEffects.Add(effect, data);
        }

        private class EffectData
        {
            public EffectData(StatusEffect effect, List<GameEntity> particleEntities, Agent applierAgent)
            {
                Effect = effect;
                ParticleEntities = particleEntities;
                ApplierAgent = applierAgent;
            }

            public List<GameEntity> ParticleEntities { get; set; }
            public StatusEffect Effect { get; set; }
            public Agent ApplierAgent { get; set; }
        }

        public class EffectAggregate
        {
            public float HealthOverTime { get; set; } = 0;
            public float WardSaveFactor { get; set; } = 0;
            public float FlatArmorEffect { get; set; } = 0;
            public float PercentageArmorEffect { get; set; } = 0;
            public float DamageOverTime { get; set; } = 0;

            public void AddEffect(StatusEffect effect)
            {
                switch (effect.Template.Type)
                {
                    case StatusEffectTemplate.EffectType.Armor:
                        FlatArmorEffect += effect.Template.FlatArmorEffect;
                        PercentageArmorEffect += effect.Template.PercentageArmorEffect;
                        WardSaveFactor += effect.Template.WardSaveFactor;
                        break;
                    case StatusEffectTemplate.EffectType.DamageOverTime:
                        DamageOverTime += effect.Template.DamageOverTime;
                        break;
                    case StatusEffectTemplate.EffectType.HealthOverTime:
                        HealthOverTime += effect.Template.HealthOverTime;
                        break;
                }
            }
        }
    }
}