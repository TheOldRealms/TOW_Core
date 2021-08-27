using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TOW_Core.Utilities.Extensions
{
    public static class SettlementExtensions
    {
        /// <summary>
        /// Returns a list of all scene names, as defined in tow_settlements.xml (or other settlements files).
        /// As seen in this image, locations have scene names, e.g. scene_name="empire_siege_001" - these 
        /// are the names returned by this method: https://imgur.com/Wh4LsxW
        /// </summary>
        /// <param name="settlement"></param>
        /// <returns></returns>
        public static List<string> GetSceneNames(this Settlement settlement)
        {
            
            List<string> sceneNames = new List<string>();
            if(settlement.LocationComplex != null)
            {
                List<Location> settlementLocations = settlement.LocationComplex.GetListOfLocations().ToList();
                foreach (Location settlementLocation in settlementLocations)
                {
                    for (int i = 0; i < settlementLocation.GetSceneCount(); i++)
                    {
                        sceneNames.Add(settlementLocation.GetSceneName(i));
                    }
                }
            }
            return sceneNames;
        }
    }
}
