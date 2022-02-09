using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;
using TOW_Core.Abilities.Crosshairs;
using TOW_Core.Battle.TriggeredEffect;

namespace TOW_Core.Abilities
{
    [Serializable]
    public class AbilityTemplate
    {
        [XmlAttribute]
        public string StringID = "";
        [XmlAttribute]
        public string Name = "";
        [XmlAttribute]
        public string SpriteName  = "";
        [XmlAttribute]
        public int CoolDown = 10;
        [XmlAttribute]
        public int WindsOfMagicCost = 0; //spell only
        [XmlAttribute]
        public float BaseMisCastChance = 0.3f; //spell only
        [XmlAttribute]
        public float Duration = 3;
        [XmlAttribute]
        public float Radius = 0.8f;
        [XmlAttribute]
        public AbilityType AbilityType = AbilityType.Spell;
        [XmlAttribute]
        public AbilityEffectType AbilityEffectType = AbilityEffectType.MovingProjectile;
        [XmlAttribute]
        public float BaseMovementSpeed = 35f;
        [XmlAttribute]
        public float TickInterval = 1f;
        [XmlAttribute]
        public TriggerType TriggerType = TriggerType.OnCollision;
        [XmlAttribute]
        public string TriggeredEffectID = "";
        [XmlAttribute]
        public bool HasLight = true;
        [XmlAttribute]
        public float LightIntensity = 5f;
        [XmlAttribute]
        public float LightRadius = 5f;
        public Vec3 LightColorRGB = new Vec3(255, 170, 0);
        [XmlAttribute]
        public float LightFlickeringMagnitude = 1;
        [XmlAttribute]
        public float LightFlickeringInterval = 0.2f;
        [XmlAttribute]
        public bool ShadowCastEnabled = true;
        [XmlAttribute]
        public string ParticleEffectPrefab = "";
        [XmlAttribute]
        public float ParticleEffectSizeModifier = 1;
        [XmlAttribute]
        public string SoundEffectToPlay = "";
        [XmlAttribute]
        public bool ShouldSoundLoopOverDuration = true;
        [XmlAttribute]
        public CastType CastType = CastType.Instant;
        [XmlAttribute]
        public float CastTime = 0;
        [XmlAttribute]
        public string AnimationActionName = "";
        [XmlAttribute]
        public AbilityTargetType AbilityTargetType = AbilityTargetType.Enemies;
        [XmlAttribute]
        public float Offset = 1.0f;
        [XmlAttribute]
        public CrosshairType CrosshairType = CrosshairType.None;
        [XmlAttribute]
        public float MinDistance = 1.0f;
        [XmlAttribute]
        public float MaxDistance = 1.0f;
        [XmlAttribute]
        public float TargetCapturingRadius = 0;
        [XmlAttribute]
        public int SpellTier = 0; //spell only, max 3 (0-1-2-3)
        [XmlAttribute]
        public string BelongsToLoreID = ""; //spell only
        public SeekerParameters SeekerParameters;
        
        public AbilityTemplate() { }
        public AbilityTemplate(string id) => StringID = id;
    }
}
