using System.Collections.Generic;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities;
using static TOW_Core.Utilities.TOWParticleSystem;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffect
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("particle_id")]
        public string ParticleId { get; set; }
        [XmlAttribute("particle_intensity")]
        public ParticleIntensity ParticleIntensity { get; set; }
        [XmlAttribute("health_over_time")]
        public float HealthOverTime { get; set; }
        [XmlAttribute("ward_save_factor")]
        public float WardSaveFactor { get; set; }
        [XmlAttribute("flat_armor_effect")]
        public float FlatArmorEffect { get; set; }
        [XmlAttribute("percentage_armor_effect")]
        public float PercentageArmorEffect { get; set; }
        [XmlAttribute("flat_damage_effect")]
        public float FlatDamageEffect { get; set; }
        [XmlAttribute("percentage_damage_effect")]
        public float PercentageDamageEffect { get; set; }
        [XmlAttribute("duration")]
        public int Duration { get; set; }
        [XmlAttribute("type")]
        public EffectType Type { get; set; }

        [XmlIgnore]
        public int CurrentDuration { get; set; }
        [XmlIgnore]
        public Agent Affector { get; set; }

        public enum EffectType
        {
            Armor,
            Health,
            Damage
        };

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() != typeof(StatusEffect))
            {
                return false;
            }
            return GetHashCode() == ((StatusEffect)obj).GetHashCode();
        }
    }
}