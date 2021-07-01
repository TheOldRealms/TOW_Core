using NLog;
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
using TOW_Core.Utilities;

namespace TOW_Core.Battle.Map
{
    public class TowMapWeatherModel : DefaultMapWeatherModel
    {
        private readonly string ModuleName = "TOW_EnvironmentAssets";
        private readonly string ForceAtmosphereKey = "forceatmo";
        private readonly float NearSettlementThreshold = 1.5f;

        //Season codes from TW code: https://imgur.com/p5CtVt0
        private readonly Dictionary<string, int> SeasonCodes = new Dictionary<string, int>()
        {
            {"spring",  0},
            {"summer", 1},
            {"fall", 2},
            {"winter", 3}
        };

        public override AtmosphereInfo GetAtmosphereModel(CampaignTime timeOfYear, Vec3 pos)
        {
            AtmosphereInfo info = base.GetAtmosphereModel(timeOfYear, pos);
            string sceneName = "";
           
            if (MainPartyIsNearSettlement())
            {
                //If the player is near (or entering) a settlement, we'll want to use that settlement's atmosphere data.
                Settlement nearestSettlement = Helpers.SettlementHelper.FindNearestSettlementToPoint(MobileParty.MainParty.Position2D);
                sceneName = GetSceneNameFromSettlement(nearestSettlement);
            }

            if(sceneName == "")
            {
                sceneName = PlayerEncounter.GetBattleSceneForMapPosition(MobileParty.MainParty.Position2D);
            }

            if (sceneName.Contains(ForceAtmosphereKey))
            {
                //Read atmosphere data from xml
                //Update info object with values from xml
                string sceneDirectoryName = Path.Combine(ModuleHelper.GetModuleFullPath(ModuleName), "SceneObj", sceneName);
                string atmosphereFileName = "atmosphere.xml";
                string[] files = Directory.GetFiles(sceneDirectoryName, atmosphereFileName, SearchOption.TopDirectoryOnly);

                if (files.Length == 0)
                {
                    TOWCommon.Log("Failed to find " + atmosphereFileName + " for atmosphere information.", LogLevel.Warn);
                }

                XmlDocument atmosphereXml = new XmlDocument();
                atmosphereXml.Load(files[0]);

                try
                {
                    info = GetUpdatedAtmosphereInfoFromXml(atmosphereXml, info);
                }
                catch (KeyNotFoundException e)
                {
                    TOWCommon.Log("Failed to parse atmosphere info from atmosphere.xml at " + sceneDirectoryName + 
                        " - reverting to original atmosphere info.", LogLevel.Error);
                    TOWCommon.Log(e.StackTrace, LogLevel.Error);
                    info = base.GetAtmosphereModel(timeOfYear, pos);
                }
            }

            return info;
        }

        /// <summary>
        /// Converts an rgb string to a Vec3 of its respective floats.
        /// </summary>
        /// <param name="colorString">A string of color values in the format found in the atmosphere xml, "[r], [g], [b]"</param>
        /// <returns></returns>
        private Vec3 GetVec3FromColorString(string colorString)
        {
            float[] colors = colorString.Split(',')
                    .ToList()
                    .Select(color => float.Parse(color.Trim()))
                    .ToArray();
            return new Vec3(colors[0], colors[1], colors[2]);
        }

        private bool MainPartyIsNearSettlement()
        {
            foreach (Settlement settlement in Settlement.All)
            {
                float distance;
                Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, MobileParty.MainParty, Campaign.MapDiagonal, out distance);
                if (distance < NearSettlementThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetSceneNameFromSettlement(Settlement settlement)
        {
            List<Location> settlementLocations = settlement.LocationComplex.GetListOfLocations().ToList();
            if (settlementLocations.Count > 0)
            {
                Location settlementLocation = settlementLocations.First();
                if (settlementLocation.GetSceneCount() > 0)
                {
                    return settlementLocation.GetSceneName(0);
                }
            }
            return "";
        }

        private Dictionary<string, string> GetPairsForFirstNodeWithTagName(XmlDocument document, string tagName)
        {
            XmlNode node = document.GetElementsByTagName(tagName).Item(0);
            Dictionary<string, string> pairs = new Dictionary<string, string>();

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode currentNode = node.ChildNodes.Item(i);
                pairs.Add(currentNode.Attributes["name"].Value, currentNode.Attributes["value"].Value);
            }

            return pairs;
        }

        private AtmosphereInfo GetUpdatedAtmosphereInfoFromXml(XmlDocument atmosphereXml, AtmosphereInfo originalInfo)
        {
            AtmosphereInfo info = originalInfo;
            //Load nodes from atmosphere xml into dicts for each major node
            Dictionary<string, string> valuePairs = GetPairsForFirstNodeWithTagName(atmosphereXml, "values");
            XmlNode globalAmbient = atmosphereXml.GetElementsByTagName("global_ambient").Item(0);
            Dictionary<string, string> fogPairs = GetPairsForFirstNodeWithTagName(atmosphereXml, "fog");
            Dictionary<string, string> sunPairs = GetPairsForFirstNodeWithTagName(atmosphereXml, "sun");
            Dictionary<string, string> postFxPairs = GetPairsForFirstNodeWithTagName(atmosphereXml, "postfx");

            info.AtmosphereName = valuePairs["name"];

            //---Update SunInformation
            SunInformation sunInfo = info.SunInfo;

            sunInfo.Altitude = float.Parse(sunPairs["sun_altitude"]);
            sunInfo.Angle = float.Parse(sunPairs["sun_angle"]);
            sunInfo.Color = GetVec3FromColorString(sunPairs["sun_color"]);

            //The XmlNode for <sun> doesn't contain a sun brightness value, but it does contain a 
            //"sun_intesity" value (yes, it's spelled intesity in the xml, lol)
            sunInfo.Brightness = float.Parse(sunPairs["sun_intesity"]);

            //sunInfo.MaxBrightness = (not in the xml)?
            sunInfo.Size = float.Parse(sunPairs["sun_size"]);
            sunInfo.RayStrength = float.Parse(sunPairs["sunshafts_strength"]);

            info.SunInfo = sunInfo;
            //---Update RainInformation
            RainInformation rainInfo = info.RainInfo;
            rainInfo.Density = float.Parse(valuePairs["fall_density"]);

            info.RainInfo = rainInfo;
            //---Update SnowInformation
            SnowInformation snowInfo = info.SnowInfo;
            snowInfo.Density = float.Parse(valuePairs["snow_density"]);

            info.SnowInfo = snowInfo;
            //---Update SkyInformation
            SkyInformation skyInfo = info.SkyInfo;
            skyInfo.Brightness = float.Parse(sunPairs["sky_brightness"]);

            info.SkyInfo = skyInfo;
            //---Update AmbientInformation
            AmbientInformation ambientInfo = info.AmbientInfo;
            ambientInfo.EnvironmentMultiplier = float.Parse(valuePairs["global_envmap_multiplier"]);
            ambientInfo.AmbientColor = GetVec3FromColorString(globalAmbient.Attributes["fog_ambient_color"].Value);
            ambientInfo.MieScatterStrength = float.Parse(fogPairs["scatter_strength"]);
            ambientInfo.RayleighConstant = float.Parse(sunPairs["rayleigh_constant"]);

            info.AmbientInfo = ambientInfo;
            //---Update FogInformation
            FogInformation fogInfo = info.FogInfo;
            fogInfo.Density = float.Parse(fogPairs["fog_density"]);
            fogInfo.Color = GetVec3FromColorString(fogPairs["fog_color"]);
            fogInfo.Falloff = float.Parse(fogPairs["fog_falloff"]);

            info.FogInfo = fogInfo;
            //---Update TimeInformation
            TimeInformation timeInfo = info.TimeInfo;
            timeInfo.TimeOfDay = float.Parse(valuePairs["time_of_day"]);
            //timeInfo.NightTimeFactor = (not in xml)?
            //timeInfo.DrynessFactor = (not in xml)?
            timeInfo.Season = SeasonCodes[valuePairs["season"]];

            info.TimeInfo = timeInfo;
            //---Update AreaInformation
            AreaInformation areaInfo = info.AreaInfo;
            areaInfo.Temperature = float.Parse(valuePairs["temperature"]);
            areaInfo.Humidity = float.Parse(valuePairs["humidity"]);
            //areaInfo.AreaType = (not in xml)?

            info.AreaInfo = areaInfo;
            //---Update PostProcessInformation
            PostProcessInformation postProInfo = info.PostProInfo;
            postProInfo.MinExposure = float.Parse(postFxPairs["min_exposure"]);
            postProInfo.MaxExposure = float.Parse(postFxPairs["max_exposure"]);
            postProInfo.BrightpassThreshold = float.Parse(postFxPairs["brightpass_threshold"]);
            postProInfo.MiddleGray = float.Parse(valuePairs["middle_gray"]);

            info.PostProInfo = postProInfo;

            return info;
        }
    }
}
