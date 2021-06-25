using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TOW_Core.Battle.ShieldPatterns
{
    public static class ShieldPatternsManager
    {
        private static Dictionary<string, List<Banner>> _patterns = new Dictionary<string, List<Banner>>();
        private static Random _random = new Random();

        public static Banner GetRandomBannerFor(string cultureId, string faction = "")
        {
            List<Banner> banners = null;
            if (faction == "")
            {
                _patterns.TryGetValue(cultureId, out banners);
            }
            else if (faction == "player_faction")
            {
                _patterns.TryGetValue(Hero.MainHero.Culture.StringId, out banners);
            }
            else
            {
                _patterns.TryGetValue(faction, out banners);
            }
            if (banners != null && banners.Count > 0)
            {
                var i = _random.Next(0, banners.Count);
                return banners[i];
            }
            else return null;
        }

        public static void LoadShieldPatterns()
        {
            try
            {
                var ser = new XmlSerializer(typeof(List<ShieldPattern>));
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_shieldpatterns.xml");
                var list = ser.Deserialize(File.OpenRead(path)) as List<ShieldPattern>;
                foreach(var item in list)
                {
                    if (!_patterns.ContainsKey(item.CultureOrKingdomId))
                    {
                        _patterns.Add(item.CultureOrKingdomId, new List<Banner>());
                    }

                    foreach(var item2 in item.BannerCodes)
                    {
                        _patterns[item.CultureOrKingdomId].Add(new Banner(item2));
                    }
                }
            }
            catch
            {
                TOW_Core.Utilities.TOWCommon.Log("Attempted to load shield patterns but failed.", NLog.LogLevel.Error);
            }
        }

        public static void WriteSampleXml()
        {
            var list = new List<ShieldPattern>();
            var tuple = new ShieldPattern();
            tuple.CultureOrKingdomId = "empire";
            tuple.BannerCodes.Add("17.127.148.1480.2012.754.759.1.0.180.505.148.148.1294.1871.2131.1127.1.0.0.506.128.128.1089.1671.781.760.1.0.0.506.142.2.956.572.782.770.1.0.0.506.149.142.762.440.780.774.1.0.0.103.14.126.229.224.1295.412.1.0.0.503.131.116.273.279.773.767.1.1.0.503.116.116.205.209.771.768.1.1.0.202.129.126.290.224.895.790.1.0.88.202.126.126.290.224.646.783.1.1.-89.457.122.126.190.147.772.979.1.1.-181");
            tuple.BannerCodes.Add("11.127.142.2382.2934.752.726.1.0.258.505.148.148.1294.1871.2131.1128.1.0.0.423.2.116.530.534.769.764.1.0.0.529.127.142.339.352.788.655.1.0.3.510.127.142.625.251.772.767.1.1.141.510.127.142.593.248.769.771.1.0.219.510.127.127.321.235.761.900.1.1.270.510.127.127.101.176.777.773.1.1.321.510.127.127.101.176.758.756.1.1.92.510.142.142.93.141.692.795.1.1.38.503.143.116.204.199.771.770.1.1.0.202.131.116.292.244.758.323.1.1.180");
            list.Add(tuple);
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_shieldpatterns.xml");
            var ser = new XmlSerializer(typeof(List<ShieldPattern>));
            ser.Serialize(File.OpenWrite(path), list);
        }
    }

    [Serializable]
    public class ShieldPattern
    {
        [XmlElement]
        public string CultureOrKingdomId { get; set; } = "";
        [XmlElement(ElementName ="BannerCode")]
        public List<string> BannerCodes { get; set; } = new List<string>();
    }
}
