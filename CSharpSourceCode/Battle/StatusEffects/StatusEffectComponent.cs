using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using Messages.FromClient.ToLobbyServer;
using TOW_Core.Utilities;
using TOW_Core.Battle.Extensions;
using TaleWorlds.Engine;
using System.Linq;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectComponent: AgentComponent
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

        public void RunStatusEffect(string id)
        {
            if(Agent == null)
                return;

            StatusEffect effect = _currentEffects.Keys.Where(e => e.Id.Equals(id)).FirstOrDefault();
            if (effect != null)
            {
                _currentEffects[effect].Duration = effect.Duration;
            }
            else
            {
                effect = StatusEffectManager.GetStatusEffect(id);
                AddEffect(effect);
            }
        }

        public void OnElapsed(float dt)
        {
            foreach (StatusEffect effect in _currentEffects.Keys.ToList())
            {
                _currentEffects[effect].Duration--;

                if (_currentEffects[effect].Duration <= 0)
                {
                    RemoveEffect(effect);
                }
            }

            //Temporary method for applying effects from the aggregate. This needs to go to a damage manager/calculator which will use the 
            //aggregated information to determine how much damage to apply to the agent
            if (Agent.IsActive() && Agent != null)
            {
                if(_effectAggregate.HealthOverTime < 0)
                {
                    Agent.ApplyDamage(-1 * ((int)_effectAggregate.HealthOverTime), Agent, causeStagger: false);
                }
                else if(_effectAggregate.HealthOverTime > 0)
                {
                    Agent.Heal((int)_effectAggregate.HealthOverTime);
                }
            }
        }
        
        public void OnTick(object sender, OnTickArgs e)
        {
            float dt = e.deltatime;
            _deltaSinceLastTick += dt;
            if(_deltaSinceLastTick > _updateFrequency)
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
            _effectAggregate.RemoveEffect(effect);
        }

        private void AddEffect(StatusEffect effect)
        {
            List<GameEntity> childEntities;
            TOWParticleSystem.ApplyParticleToAgent(Agent, effect.ParticleId, out childEntities, effect.ParticleIntensity);

            EffectData data = new EffectData(effect);
            data.ParticleEntities = childEntities;

            _currentEffects.Add(effect, data);
            _effectAggregate.AddEffect(effect);
        }

        private class EffectData
        {
            public EffectData(StatusEffect effect)
            {
                Duration = effect.Duration;
                Effect = effect;
            }

            public int Duration { get; set; }
            public List<GameEntity> ParticleEntities { get; set; }
            public StatusEffect Effect { get; set; }
        }

        private class EffectAggregate
        {
            public float HealthOverTime { get; set; } = 0;
            public float WardSaveFactor { get; set; } = 0;
            public float FlatArmorEffect { get; set; } = 0;
            public float PercentageArmorEffect { get; set; } = 0;
            public float FlatDamageEffect { get; set; } = 0;
            public float PercentageDamageEffect { get; set; } = 0;

            public void AddEffect(StatusEffect effect)
            {
                switch (effect.Type)
                {
                    case StatusEffect.EffectType.Armor:
                        FlatArmorEffect += effect.FlatArmorEffect;
                        PercentageArmorEffect += effect.PercentageArmorEffect;
                        WardSaveFactor += effect.WardSaveFactor;
                        break;
                    case StatusEffect.EffectType.Damage:
                        FlatDamageEffect += effect.FlatDamageEffect;
                        PercentageDamageEffect += effect.PercentageDamageEffect;
                        break;
                    case StatusEffect.EffectType.Health:
                        HealthOverTime += effect.HealthOverTime;
                        break;
                }
            }

            public void RemoveEffect(StatusEffect effect)
            {
                switch (effect.Type)
                {
                    case StatusEffect.EffectType.Armor:
                        FlatArmorEffect -= effect.FlatArmorEffect;
                        PercentageArmorEffect -= effect.PercentageArmorEffect;
                        WardSaveFactor -= effect.WardSaveFactor;
                        break;
                    case StatusEffect.EffectType.Damage:
                        FlatDamageEffect -= effect.FlatDamageEffect;
                        PercentageDamageEffect -= effect.PercentageDamageEffect;
                        break;
                    case StatusEffect.EffectType.Health:
                        HealthOverTime -= effect.HealthOverTime;
                        break;
                }
            }
        }
    }
}