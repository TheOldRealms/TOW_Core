using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.MountAndBlade;
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
        [XmlAttribute("ward_save_factor")]
        public float WardSaveFactor { get; set; } = 0;
        [XmlAttribute("flat_armor_effect")]
        public float FlatArmorEffect { get; set; } = 0;
        [XmlAttribute("percentage_armor_effect")]
        public float PercentageArmorEffect { get; set; } = 0;
        [XmlAttribute("damage_over_time")]
        public float DamageOverTime { get; set; } = 0;
        [XmlAttribute("duration")]
        public int BaseDuration { get; set; } = 0;
        [XmlAttribute("type")]
        public EffectType Type { get; set; } = EffectType.Invalid;

        public enum EffectType
        {
            Armor,
            HealthOverTime,
            DamageOverTime,
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
