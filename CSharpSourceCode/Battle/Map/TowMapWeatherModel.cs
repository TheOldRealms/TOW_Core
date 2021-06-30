using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace TOW_Core.Battle.Map
{
    public class TowMapWeatherModel : DefaultMapWeatherModel
    {
        private readonly string ModuleName = "TOW_EnvironmentAssets";

        public override AtmosphereInfo GetAtmosphereModel(CampaignTime timeOfYear, Vec3 pos)
        {
            AtmosphereInfo info = base.GetAtmosphereModel(timeOfYear, pos);

            string sceneName = PlayerEncounter.GetBattleSceneForMapPosition(MobileParty.MainParty.Position2D);

            string sceneDirectoryName = Path.Combine(ModuleHelper.GetModuleFullPath(ModuleName), "SceneObj", sceneName);
            if(sceneName.Contains("TOW_empire_village_001"))
            {
                string[] files = Directory.GetFiles(sceneDirectoryName, "atmosphere.xml", SearchOption.TopDirectoryOnly);
                Dictionary<string, List<string>> attributesDictionary = new Dictionary<string, List<string>>();

                XmlDocument atmosphereXml = new XmlDocument();
                atmosphereXml.Load(files[0]);
                XmlNode values = atmosphereXml.GetElementsByTagName("values").Item(0);
                XmlNode globalAmbient = atmosphereXml.GetElementsByTagName("global_ambient").Item(0);
                XmlNode fog = atmosphereXml.GetElementsByTagName("fog").Item(0);
                XmlNode cloudShadow = atmosphereXml.GetElementsByTagName("cloud_shadow").Item(0);
                XmlNode sun = atmosphereXml.GetElementsByTagName("sun").Item(0);
                XmlNodeList sunInfoNodes = sun.ChildNodes;
                XmlNode postfx = atmosphereXml.GetElementsByTagName("postfx").Item(0);
                XmlNode cubemapTexture = atmosphereXml.GetElementsByTagName("cubemap_texture").Item(0);
                XmlNode flags = atmosphereXml.GetElementsByTagName("flags").Item(0);

                SunInformation sunInfo = info.SunInfo;

                sunInfo.Altitude = float.Parse(sunInfoNodes.Item(1).Attributes["value"].Value);
                sunInfo.Size = float.Parse(sunInfoNodes.Item(9).Attributes["value"].Value);
                sunInfo.Angle = float.Parse(sunInfoNodes.Item(2).Attributes["value"].Value);
                float color1 = float.Parse(sunInfoNodes.Item(12).Attributes["value"].Value.Split(',')[0].Trim());
                float color2 = float.Parse(sunInfoNodes.Item(12).Attributes["value"].Value.Split(',')[1].Trim());
                float color3 = float.Parse(sunInfoNodes.Item(12).Attributes["value"].Value.Split(',')[2].Trim());
                sunInfo.Color = new Vec3(color1, color2, color3);
                info.SunInfo = sunInfo;
                info.TimeInfo.NightTimeFactor = 10000;
                info.TimeInfo.TimeOfDay = 0;
            }

            return info;
        }


    }
}
