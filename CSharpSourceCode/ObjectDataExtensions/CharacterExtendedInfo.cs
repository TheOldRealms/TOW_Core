using System.Collections.Generic;
using System.Xml.Serialization;
using TOW_Core.Battle.Damage;
using System;


namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Contains Tow data of single unit or character template. 
    /// </summary>
    public class CharacterExtendedInfo
    {
        [XmlAttribute("id")]
        public string CharacterStringId;
        [XmlAttribute("VoiceClassName")]
        public string VoiceClassName = "none";
        [XmlArray("Abilities")]
        public List<string> Abilities = new List<string>();
        [XmlArray("Attributes")]
        public List<string> CharacterAttributes = new List<string>();
        [XmlArray("DamageProportions")]
        public List<DamageProportionTuple> DamageProportions = new List<DamageProportionTuple>();
        [XmlArray("Resistances")]
        public List<ResistanceTuple> Resistances = new List<ResistanceTuple>();
        [XmlArray("DamageAmplifiers")]
        public List<AmplifierTuple> DamageAmplifiers = new List<AmplifierTuple>();
    }

    [Serializable]
    public class ResistanceTuple
    {
        [XmlAttribute]
        public DamageType ResistedDamageType = DamageType.Invalid;
        [XmlAttribute]
        public float ReductionPercent = 0;
    }

    [Serializable]
    public class AmplifierTuple
    {
        [XmlAttribute]
        public DamageType AmplifiedDamageType = DamageType.Invalid;
        [XmlAttribute]
        public float DamageAmplifier = 0;
    }

    [Serializable]
    public class DamageProportionTuple
    {
        [XmlAttribute]
        public DamageType DamageType = DamageType.Invalid;
        [XmlAttribute]
        public float Percent = 1;
        public DamageProportionTuple()
        {
        }
        public DamageProportionTuple(DamageType damageType, float percent)
        {
            DamageType = damageType;
            Percent = percent;
        }
    }
    
    /// <summary>
    /// Contains summarized Agent properties for Damage and Resistances. Cannot be changed.
    /// </summary>
    public struct AgentPropertyContainer
    {
        public readonly float[] DamageProportions;
        public readonly float[] DamagePercentages;
        public readonly float[] ResistancePercentages;
        public readonly float[] AdditionalDamagePercentages;
        public AgentPropertyContainer(float[] damageProportions, float[] damagePercentages, float[] resistancePercentages, float[] additionalDamagePercentages)
        {
            DamageProportions = damageProportions;
            DamagePercentages = damagePercentages;
            ResistancePercentages = resistancePercentages;
            AdditionalDamagePercentages = additionalDamagePercentages;
        }
    }
    
    public enum PropertyMask : int
    {
        Attack=0,
        Defense=1,
        All=2
    }
}