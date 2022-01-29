using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using System.Xml.Serialization;
using TOW_Core.Battle.Damage;
using TaleWorlds.Library;
using System.IO;
using System;
using System.Net.NetworkInformation;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using Path = System.IO.Path;

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
        public DamageType DamageType = DamageType.Physical;
        [XmlAttribute]
        public float Percent = 1;
    }


    public struct AgentPropertyContainer
    {
        public float[] DamageProportions;
        public float[] DamagePercentages;
        public float[] ResistancePercentages;

        public AgentPropertyContainer(float physicalDamage, float fireDamage, float lightningDamage, float holyDamage, float magicDamage,
         float phyiscalResistance, float fireResistance, float lightningResistance, float holyResistance, float magicResistance)
        {
            DamagePercentages = new[]
            {
                physicalDamage, fireDamage, lightningDamage, holyDamage, magicDamage
            };
            ResistancePercentages = new[]
            {
                phyiscalResistance, fireResistance, lightningResistance, holyResistance, magicResistance
            };
            DamageProportions = new float[7];
        }
        
        public AgentPropertyContainer(float[] damageProportions, float[] damagePercentages, float[] resistancePercentages, DamageType damageType=DamageType.Physical)
        {
            DamageProportions = damageProportions;
            DamagePercentages = damagePercentages;
            ResistancePercentages = resistancePercentages;
        }
        
    }
    
    public enum PropertyFlag : int
    {
        Attack=0,
        Defense=1,
        All=2
    }
}