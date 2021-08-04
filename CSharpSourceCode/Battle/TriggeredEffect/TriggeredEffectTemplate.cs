using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TOW_Core.Battle.Damage;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Engine;
using TOW_Core.Abilities;

namespace TOW_Core.Battle.TriggeredEffect
{
    [Serializable]
    public class TriggeredEffectTemplate
    {
        [XmlAttribute]
        public string StringID = "";
        [XmlAttribute]
        public string BurstParticleEffectPrefab = "";
        [XmlAttribute]
        public string SoundEffectId = "";
        [XmlAttribute]
        public float SoundEffectLength = 2.5f;
        [XmlAttribute]
        public DamageType DamageType = DamageType.Fire;
        [XmlAttribute]
        public int DamageAmount = 50;
        [XmlAttribute]
        public float Radius = 5;
        [XmlAttribute]
        public bool HasShockWave = false;
        [XmlAttribute]
        public TargetType TargetType = TargetType.Enemy;
        [XmlAttribute]
        public string ImbuedStatusEffectID = "";
        [XmlAttribute]
        public float ImbuedStatusEffectDuration = 5f;
        [XmlAttribute]
        public float DamageVariance = 0.2f;
        [XmlAttribute]
        public string ScriptNameToTrigger = "";
    }
}
