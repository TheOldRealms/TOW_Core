using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TOW_Core.Texts
{
    public class TOWTextOverride
    {
        [XmlAttribute]
        public string StringID { get; set; } = "str_culture_rich_name.khuzait";
        [XmlAttribute]
        public string TextOverride { get; set; } = "Vampire Counts";
    }
}
