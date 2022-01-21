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
        public DefenseProperty DefenseProperty { get; set; }
        [XmlElement]
        public OffenseProperty OffenseProperty { get; set; }
        [XmlAttribute]
        public string OnHitScriptName { get; set; } = "none";
        [XmlAttribute]
        public string ImbuedStatusEffectId { get; set; } = "none";
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
}
