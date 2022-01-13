using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TOW_Core.Battle.Damage
{
    [Serializable]
    public class ItemDamageProperty
    {
        [XmlAttribute]
        public int MinDamage;
        [XmlAttribute]
        public int MaxDamage;
        [XmlAttribute]
        public DamageType DamageType;
    }
}
