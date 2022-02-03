using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.MountAndBlade;
using TOW_Core.Battle.Damage;
using TOW_Core.ObjectDataExtensions;
using static TOW_Core.Utilities.TOWParticleSystem;

namespace TOW_Core.Battle.StatusEffects
{
    public class StatusEffectTemplate
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("particle_id")]
        public string ParticleId { get; set; }
        [XmlAttribute("particle_intensity")]
        public ParticleIntensity ParticleIntensity { get; set; }
        [XmlAttribute("health_over_time")]
        public float HealthOverTime { get; set; } = 0;
        [XmlAttribute("duration")]
        public int BaseDuration { get; set; } = 0;
        [XmlAttribute("type")]
        public EffectType Type { get; set; } = EffectType.Invalid;
        [XmlAttribute("damage_over_time")]
        public float DamageOverTime { get; set; } = 0;
        [XmlAttribute("damage_type")]
        public DamageType DamageType { get; set; } = DamageType.Physical;
        [XmlElement]
        public AmplifierTuple DamageReduction { get; set; } = new AmplifierTuple();
        public enum EffectType
        {
            HealthOverTime,
            DamageOverTime,
            Reduction,
            Invalid
        };

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(StatusEffect))
            {
                return false;
            }
            return GetHashCode() == ((StatusEffect)obj).GetHashCode();
        }
    }
}
