using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Items
{
    [Serializable]
    public class ItemTrait
    {
        [XmlAttribute]
        public string ItemTraitName { get; set; }
        [XmlElement]
        public string ItemTraitDescription { get; set; }
        [XmlElement]
        public ResistanceTuple ResistanceTuple { get; set; }
        [XmlElement]
        public AmplifierTuple AmplifierTuple { get; set; }
        [XmlElement]
        public DamageProportionTuple AdditionalDamageTuple { get; set; }
        [XmlElement]
        public SkillTuple SkillTuple { get; set; }
        [XmlAttribute]
        public string OnHitScriptName { get; set; } = "none";
        [XmlAttribute]
        public string ImbuedStatusEffectId { get; set; } = "none";
        [XmlAttribute]
        public string IconName { get; set; } = "none";
        [XmlElement]
        public WeaponParticlePreset WeaponParticlePreset { get; set; }
    }

    [Serializable]
    public class WeaponParticlePreset
    {
        [XmlAttribute]
        public string ParticlePrefab { get; set; } = "invalid";
        [XmlAttribute]
        public bool IsUniqueSingleCopy { get; set; } = false;
    }

    [Serializable]
    public class SkillTuple
    {
        [XmlAttribute]
        public bool IsAbility { get; set; } = false;
        [XmlAttribute]
        public string SkillId { get; set; }
        [XmlAttribute]
        public float SkillExp { get; set; } = 0;
        [XmlAttribute]
        public float LearningTime { get; set; } = 1;
        
    }
}