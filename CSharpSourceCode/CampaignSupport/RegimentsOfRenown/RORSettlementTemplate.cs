using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TOW_Core.CampaignSupport.RegimentsOfRenown
{
    [Serializable]
    public class RORSettlementTemplate
    {
        [XmlAttribute]
        public string SettlementId = "invalid";
        [XmlAttribute]
        public string BaseTroopId = "invalid";
        [XmlAttribute]
        public string MenuHeaderText = "invalid";
        [XmlAttribute]
        public string MenuBackgroundImageId = "invalid";
        [XmlAttribute]
        public string RegimentName = "invalid";
        [XmlAttribute]
        public string RegimentHQName = "invalid";
    }
}
