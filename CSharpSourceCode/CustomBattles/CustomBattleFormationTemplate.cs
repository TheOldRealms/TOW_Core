using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;

namespace TOW_Core.CustomBattles
{
    public class CustomBattleFormationTemplate
    {
        [XmlAttribute]
        public string cultureId { get; set; } = "empire";
        [XmlAttribute]
        public FormationClass formation { get; set; } = FormationClass.Infantry;
        [XmlAttribute]
        public string troopId { get; set; } = "tow_empire_recruit";
    }
}
