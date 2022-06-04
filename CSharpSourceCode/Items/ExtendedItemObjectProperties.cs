using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TOW_Core.Battle.Damage;
using TOW_Core.ObjectDataExtensions;

namespace TOW_Core.Items
{
    [Serializable]
    public class ExtendedItemObjectProperties
    {
        [XmlAttribute]
        public string ItemStringId;
        [XmlElement]
        public string Description = "";
        [XmlArray("DamageProportions")]
        public List<DamageProportionTuple> DamageProportions = new List<DamageProportionTuple>();
        [XmlArray("ItemTraits")]
        public List<ItemTrait> ItemTraits = new List<ItemTrait>();

        public ExtendedItemObjectProperties() { }

        public ExtendedItemObjectProperties(string id, DamageType defaultDamageType= DamageType.Physical)
        {
            ItemStringId = id;
            DamageProportionTuple proportionTuple = new DamageProportionTuple()
            {
                DamageType = defaultDamageType,
                Percent = 1f
            };
            DamageProportions.Add(proportionTuple);
        }
        public static ExtendedItemObjectProperties CreateDefault(string id)
        {
            return new ExtendedItemObjectProperties(id);
        }

        public ExtendedItemObjectProperties Clone()
        {
            var prop = new ExtendedItemObjectProperties();
            prop.ItemStringId = ItemStringId;
            prop.Description = Description;
            prop.ItemTraits = new List<ItemTrait>();
            foreach(var trait in ItemTraits)
            {
                prop.ItemTraits.Add(trait);
            }
            prop.DamageProportions = new List<DamageProportionTuple>();
            foreach(var item in DamageProportions)
            {
                prop.DamageProportions.Add(item);
            }
            return prop;
        }
    }
}
