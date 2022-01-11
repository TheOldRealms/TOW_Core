using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.Damage;

namespace TOW_Core.Items
{
    [Serializable]
    public class ExtendedItemObjectProperties
    {
        [XmlAttribute]
        public string ItemStringId;
        [XmlElement]
        public ItemDamageProperty ItemDamageProperty = null;
        [XmlArray]
        public List<ItemTrait> Traits = new List<ItemTrait>();

        public ExtendedItemObjectProperties() { }

        public ExtendedItemObjectProperties(string id) => ItemStringId = id;

        public static ExtendedItemObjectProperties CreateDefault(string id)
        {
            var item = MBObjectManager.Instance.GetObject<ItemObject>(id);
            var props = new ExtendedItemObjectProperties(id);
            if(item != null && item.HasWeaponComponent)
            {
                props.ItemDamageProperty = new ItemDamageProperty { DamageType = DamageType.Physical, MinDamage = 0 , MaxDamage = 500};
            }
            return props;
        }
    }
}
