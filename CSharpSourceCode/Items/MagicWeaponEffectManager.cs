using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TOW_Core.Items
{
    public static class MagicWeaponEffectManager
    {
        private static Dictionary<string, MagicWeaponEffect> _dictionary = new Dictionary<string, MagicWeaponEffect>();

        internal static MagicWeaponEffect GetEffectForItem(string id)
        {
            return _dictionary.ContainsKey(id) ? _dictionary[id] : null;
        }

        public static void LoadXML()
        {
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_magicweaponeffects.xml");
            if (File.Exists(path))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<MagicWeaponEffect>));
                List<MagicWeaponEffect> list = ser.Deserialize(File.OpenRead(path)) as List<MagicWeaponEffect>;
                if(list != null && list.Count > 0)
                {
                    foreach(var item in list)
                    {
                        _dictionary.Add(item.WeaponItemId, item);
                    }
                }
            }
        }
    }
}
