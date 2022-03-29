using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TOW_Core.CampaignSupport.RegimentsOfRenown
{
    public static class RORManager
    {
        private static Dictionary<string, RORSettlementTemplate> _rorSettlementTemplates = new Dictionary<string, RORSettlementTemplate>();
        private static string _templateFileName = "tow_ror_settlement_templates.xml";

        public static RORSettlementTemplate GetTemplateFor(string settlementId)
        {
            RORSettlementTemplate ror = null;
            _rorSettlementTemplates.TryGetValue(settlementId, out ror);
            return ror;
        }

        public static void LoadTemplates()
        {
            var ser = new XmlSerializer(typeof(List<RORSettlementTemplate>));
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/" + _templateFileName);
            if (File.Exists(path))
            {
                var list = ser.Deserialize(File.OpenRead(path)) as List<RORSettlementTemplate>;
                foreach (var item in list)
                {
                    _rorSettlementTemplates.Add(item.SettlementId, item);
                }
            }
        }
    }
}
