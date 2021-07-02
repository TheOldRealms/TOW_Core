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
        public static List<string> GetSceneNames(this Settlement settlement)
        {
            List<Location> settlementLocations = settlement.LocationComplex.GetListOfLocations().ToList();
            List<string> sceneNames = new List<string>();

            foreach(Location settlementLocation in settlementLocations)
            {
                for(int i=0; i<settlementLocation.GetSceneCount(); i++)
                {
                    sceneNames.Add(settlementLocation.GetSceneName(i));
                }
            }
            
            return sceneNames;
        }
    }
}
