using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TOW_Core.Items
{
    [Serializable]
    public class MagicWeaponEffect
    {
        [XmlAttribute]
        public string WeaponItemId { get; set; }
        [XmlAttribute]
        public string ParticlePrefab { get; set; }
        [XmlAttribute]
        public string OnHitScriptName { get; set; }
        [XmlAttribute]
        public string ImbuedStatusEffectId { get; set; }
        [XmlAttribute]
        public float ParticlesStartOffset { get; set; }
        [XmlAttribute]
        public float ParticlesEndOffset { get; set; }
        [XmlAttribute]
        public int NumberOfParticleSystems { get; set; }
    }
}
