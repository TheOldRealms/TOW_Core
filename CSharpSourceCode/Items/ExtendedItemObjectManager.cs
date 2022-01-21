using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Items
{
    public static class ExtendedItemObjectManager
    {
        private static Dictionary<string, ExtendedItemObjectProperties> _itemToInfoMap = new Dictionary<string, ExtendedItemObjectProperties>();
        private static string XMLPath = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_extendeditemproperties.xml");


        internal static ExtendedItemObjectProperties GetAdditionalProperties(string itemId)
        {
            ExtendedItemObjectProperties info = null;
            _itemToInfoMap.TryGetValue(itemId, out info);
            if(info != null) info = info.Clone();
            return info;
        }

        public static void LoadXML()
        {
            if (File.Exists(XMLPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<ExtendedItemObjectProperties>));
                List<ExtendedItemObjectProperties> list = ser.Deserialize(File.OpenRead(XMLPath)) as List<ExtendedItemObjectProperties>;
                if(list != null && list.Count > 0)
                {
                    foreach(var item in list)
                    {
                        _itemToInfoMap.Add(item.ItemStringId, item);
                    }
                }
            }
        }
    }
}
