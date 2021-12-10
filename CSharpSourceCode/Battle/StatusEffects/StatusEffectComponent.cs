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

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectComponent: AgentComponent
    {
        private float _updateFrequency = 1;
        private float _deltaSinceLastTick = (float)TOWMath.GetRandomDouble(0, 0.1);
        private Dictionary<StatusEffect, EffectData> _currentEffects;
        private EffectAggregate _effectAggregate;
        private Dictionary<Agent,float> _damagingAffectors;
        private Agent dominantAffector;
        private bool _isRenderedDisabled;

        public StatusEffectComponent(Agent agent) : base(agent)
        {
            _currentEffects = new Dictionary<StatusEffect, EffectData>();
            _effectAggregate = new EffectAggregate();
            _damagingAffectors = new Dictionary<Agent, float>();
            dominantAffector = this.Agent;
        }
        
        

        public void RunStatusEffect(string id, Agent affector=null)
        {
            if(Agent == null)
                return;
            
            StatusEffect effect = _currentEffects.Keys.Where(e => e.Id.Equals(id)).FirstOrDefault();
            if (effect != null)
            {
                effect.CurrentDuration= effect.Duration;
            }
            else
            {
                //
                effect = StatusEffectManager.GetStatusEffect(id);
              //  TOWCommon.Say(effect.Duration+ "is added");
                effect.CurrentDuration= effect.Duration;


                if (affector != null)
                {
                   CheckAffectorsForNewEntry(affector,effect.FlatDamageEffect);
                }
                
                AddEffect(effect);
            }
        }

        public void OnElapsed(float dt)
        {
            if (_currentEffects.IsEmpty()||_isRenderedDisabled)
            {
                return;
            }

            Agent affector = null;
            
            foreach (StatusEffect effect in _currentEffects.Keys.ToList())
            {
                effect.CurrentDuration-=dt;

                if (effect.CurrentDuration <= 0)
                {
                    RemoveEffect(effect);
                    //TOWCommon.Say("effect has gone");
                    return;
                }
                
                if (effect.CurrentDuration > 0)
                {
                    //TOWCommon.Say(effect.CurrentDuration.ToString());
                }
            }
            
            //Temporary method for applying effects from the aggregate. This needs to go to a damage manager/calculator which will use the 
            //aggregated information to determine how much damage to apply to the agent
            if (Agent.IsActive() && Agent != null && !Agent.IsFadingOut())
            {
                if(_effectAggregate.HealthOverTime < 0)
                {
                   // TOWCommon.Say("apply damage :" +(int)_effectAggregate.HealthOverTime );
                   if (dominantAffector != null)
                   {
                       Agent.ApplyDamage(-1 * ((int)_effectAggregate.HealthOverTime), dominantAffector, false, false);
                   }
                   else
                   {
                       Agent.ApplyDamage(-1 * ((int)_effectAggregate.HealthOverTime), null, false, false);
                   }
                    
                }
                else if(_effectAggregate.HealthOverTime > 0)
                {
                    Agent.Heal((int)_effectAggregate.HealthOverTime);
                }
                
            }
            

            
        }
        

        public bool IsDisabled()
        {
            return _isRenderedDisabled;
        }
        public void RenderDisabled(bool state)
        {
            _isRenderedDisabled = state;
        }
        
        public void OnTick(float dt)
        {
            _deltaSinceLastTick += dt;
            if(_deltaSinceLastTick > _updateFrequency)
            {
                _deltaSinceLastTick = (float)TOWMath.GetRandomDouble(0, 0.1);
                OnElapsed(_updateFrequency);
            }
        }
        
        
        private void CheckAffectorsForNewEntry(Agent agent, float damage)
        {
            if(_damagingAffectors.ContainsKey(agent))
            {
                if (damage > _damagingAffectors[agent])
                {
                    _damagingAffectors[agent]=damage;
                }
            }
            else
            {
                _damagingAffectors.Add(agent,damage);
            }

            dominantAffector = _damagingAffectors.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        }

        private void RemoveAffectorFromDictionary(Agent agent)
        {
            if (dominantAffector == agent)
            {
                dominantAffector = null;
                if (!_damagingAffectors.IsEmpty())
                {
                    dominantAffector = _damagingAffectors.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                }
            }
            
            if (_damagingAffectors.ContainsKey(agent))
            {
                _damagingAffectors.Remove(agent);
            }

            
        }

        private void RemoveEffect(StatusEffect effect)
        {
            if (effect.Affector != null)
            {
                RemoveAffectorFromDictionary(effect.Affector);
            }
            
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
            

            public bool hasDamagingAffector;

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