using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TOW_Core.Battle.TriggeredEffect
{
    public class TriggeredEffectManager
    {
        private static Dictionary<string, TriggeredEffectTemplate> _dictionary = new Dictionary<string, TriggeredEffectTemplate>();
        private static string _filename = "tow_triggeredeffects.xml";

        internal static TriggeredEffect CreateNew(string id)
        {
            TriggeredEffect effect = null;
            if (_dictionary.ContainsKey(id))
            {
                effect = new TriggeredEffect(_dictionary[id]);
            }
            return effect;
        }

        public static void LoadTemplates()
        {
            var ser = new XmlSerializer(typeof(List<TriggeredEffectTemplate>));
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/" + _filename);
            if (File.Exists(path))
            {
                var list = ser.Deserialize(File.OpenRead(path)) as List<TriggeredEffectTemplate>;
                foreach (var item in list)
                {
                    _dictionary.Add(item.StringID, item);
                }
            }
        }
    }
}
