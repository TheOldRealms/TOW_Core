using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using System.Xml.Serialization;
using TOW_Core.Battle.Damage;
using TaleWorlds.Library;
using System.IO;
using System;

namespace TOW_Core.ObjectDataExtensions
{
    /// <summary>
    /// Contains Tow data of single unit or character template. 
    /// </summary>
    public class CharacterExtendedInfo
    {
        [XmlAttribute("TroopId")]
        public string CharacterStringId;
        [XmlAttribute("VoiceClassName")]
        public string VoiceClassName;
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

    public class DefenseProperty
    {
        public DamageType DamageType;
        public float DamageResistance;
        public bool IsWardSave = false;
    }

    public class OffenseProperty
    {
        public DamageType DamageType;
        public float DamageBonusPercent;
        public float ArmorPenetration;
    }
}