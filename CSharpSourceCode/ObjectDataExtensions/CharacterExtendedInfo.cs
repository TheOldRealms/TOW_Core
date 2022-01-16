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
        [XmlArray("DefensiveProperties")]
        public List<DefenseProperty> Defense = new List<DefenseProperty>();
        [XmlArray("OffensiveProperties")]
        public List<OffenseProperty> Offense = new List<OffenseProperty>();

        public static void WriteToXml(List<CharacterExtendedInfo> infosToWrite)
        {
            if(infosToWrite != null)
            {
                try
                {
                    var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_extendedunitproperties.xml");
                    var ser = new XmlSerializer(typeof(List<CharacterExtendedInfo>));
                    var stream = File.OpenWrite(path);
                    ser.Serialize(stream, infosToWrite);
                    stream.Close();
                }
                catch (Exception e)
                {
                    throw e; //TODO handle this more gracefully.
                }
            }
        }
    }

    [Serializable]
    public class DefenseProperty
    {
        [XmlAttribute]
        public DamageType ResistedDamageType = DamageType.Invalid;
        [XmlAttribute]
        public DefenseType DefenseType = DefenseType.None;
        [XmlAttribute]
        public float ReductionPercent = 0;
    }

    public enum DefenseType
    {
        None,
        WardSave,
        Resistance
    }

    [Serializable]
    public class OffenseProperty
    {
        [XmlAttribute]
        public DamageType DefaultDamageTypeOverride = DamageType.Physical;  // todo name is missleading
        [XmlAttribute]
        public float BonusDamagePercent = 0;    //todo remove Bonus
        [XmlAttribute]
        public float ArmorPenetration = 0;
    }


    public struct AgentPropertyContainer
    {
        public float[] DamagePercentages;
        public float[] ResistancePercentages;
        public int ArmorPenetration;
        public float Amplifier;
        public float WardSave;
        public DamageType DamageType;

        public AgentPropertyContainer(float physicalDamage, float fireDamage, float lightningDamage, float holyDamage, float magicDamage,
         float phyiscalResistance, float fireResistance, float lightningResistance, float holyResistance, float magicResistance, int armorPenetration, float amplifier, float wardSave, DamageType damageType=DamageType.Physical)
        {
            DamagePercentages = new[]
            {
                physicalDamage, fireDamage, lightningDamage, holyDamage, magicDamage
            };
            ResistancePercentages = new[]
            {
                phyiscalResistance, fireResistance, lightningResistance, holyResistance, magicResistance
            };
            
            ArmorPenetration = armorPenetration;
            Amplifier = amplifier;
            WardSave = wardSave;
            DamageType = damageType;
        }
        
        public AgentPropertyContainer(float[] damagePercentages, float[] resistancePercentages, int armorPenetration,
            float amplfier, float wardSave, DamageType damageType=DamageType.Physical)
        {
            DamagePercentages = damagePercentages;
            ResistancePercentages = resistancePercentages;
            ArmorPenetration = armorPenetration;
            Amplifier = amplfier;
            WardSave = wardSave;
            DamageType = damageType;
        }



     
        
    }
    
    public enum PropertyFlag : int
    {
        Attack=0,
        Defense=1,
        All=2
    }
}