using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TOW_Core.Battle.Damage;

namespace TOW_Core.Items
{
    [Serializable]
    public class ExtendedItemObjectProperties
    {
        [XmlAttribute]
        public string ItemStringId;
        [XmlElement]
        public ItemDamageProperty ItemDamageProperty = new ItemDamageProperty { DamageType = DamageType.Physical, MinDamage = 0, MaxDamage = 500 };
        [XmlArray]
        public List<ItemTrait> Traits = new List<ItemTrait>();

        public ExtendedItemObjectProperties() { }

        public ExtendedItemObjectProperties(string id) => ItemStringId = id;

        public static ExtendedItemObjectProperties CreateDefault(string id)
        {
            return new ExtendedItemObjectProperties(id);
        }
    }
}
