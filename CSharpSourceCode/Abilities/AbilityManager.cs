using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;
using TOW_Core.Texts;
using TOW_Core.Utilities;

namespace TOW_Core.Abilities
{
    internal static class AbilityManager
    {
        private static Dictionary<string, List<string>> _abilities = new Dictionary<string, List<string>>();

        internal static List<string> GetAbilitesForCharacter(string id)
        {
            List<string> list = new List<string>();
            _abilities.TryGetValue(id, out list);
            return list;
        }

        internal static List<string> GetAllAbilities()
        {
            var list = new List<string>();
            foreach(var item in _abilities)
            {
                foreach(var ability in item.Value)
                {
                    if (!list.Contains(ability)) list.Add(ability);
                }
            }
            return list;
        }

        internal static void LoadAbilities()
        {
            try
            {
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_abilities.xml");
                if (File.Exists(path))
                {
                    var ser = new XmlSerializer(typeof(List<CharacterAbilityTuple>));
                    var list = ser.Deserialize(File.OpenRead(path)) as List<CharacterAbilityTuple>;
                    foreach (var item in list)
                    {
                        _abilities.Add(item.CharacterID, item.Abilities);
                    }
                }
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.ToString(), LogLevel.Error);
                throw e; //TODO handle this more gracefully.
            }
        }

        internal static void WriteSampleXML()
        {
            var list = new List<CharacterAbilityTuple>();
            var tuple = new CharacterAbilityTuple();
            tuple.CharacterID = "karlfranz";
            tuple.Abilities.Add(typeof(FireBallAbility).FullName);
            tuple.Abilities.Add("FireBallAbility");
            list.Add(tuple);
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_abilities.xml");
            var ser = new XmlSerializer(typeof(List<CharacterAbilityTuple>));
            ser.Serialize(File.OpenWrite(path), list);
        }
    }

    [Serializable]
    public class CharacterAbilityTuple
    {
        [XmlAttribute]
        public string CharacterID { get; set; } = "";
        [XmlElement]
        public List<string> Abilities { get; set; } = new List<string>();
    }
}
