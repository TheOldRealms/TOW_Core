using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using NLog;
using TOW_Core.Utilities;

namespace TOW_Core.CustomBattles
{
    internal static class CustomBattleTroopManager
    {
        private static List<CustomBattleFormationTemplate> _templates = new List<CustomBattleFormationTemplate>();

        /// <summary>
        /// Loads TOW specific XML from Moduledata to drive the troop loadouts in custom battle.
        /// </summary>
        internal static void LoadCustomBattleTroops()
        {
            try
            {
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_custombattletemplates.xml");
                if (File.Exists(path))
                {
                    var ser = new XmlSerializer(typeof(List<CustomBattleFormationTemplate>));
                    var list = ser.Deserialize(File.OpenRead(path)) as List<CustomBattleFormationTemplate>;
                    if(list != null && list.Count > 0)
                    {
                        CustomBattleTroopManager._templates = list;
                    }
                }
            }
            catch (Exception e)
            {
                TOWCommon.Log(e.ToString(), LogLevel.Error);
                throw e; //TODO handle this more gracefully.
            }
        }

        /// <summary>
        /// Gets the Tow specific troop template for the given culture and formation. Returns null if there is no entries for the given arguments.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="formation"></param>
        /// <returns></returns>
        internal static BasicCharacterObject GetTroopObjectFor(BasicCultureObject culture, FormationClass formation)
        {
            var match = CustomBattleTroopManager._templates.Where(x => x.cultureId == culture.StringId && x.formation == formation);
            if(match != null && match.Count() > 0)
            {
                try
                {
                    //this means we always return the first occurance in the templates xml. If there are multiple entires, we might want to consider a random chance between them all.
                    return MBObjectManager.Instance.GetObject<BasicCharacterObject>(match.First().troopId);
                }
                catch (Exception e)
                {
                    TOWCommon.Log(e.ToString(), LogLevel.Error);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Utility function to write override XML sample to generate the correct schema.
        /// </summary>
        internal static void WriteSampleOverrideXml()
        {
            var list = new List<CustomBattleFormationTemplate>();
            list.Add(new CustomBattleFormationTemplate());
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_custombattletemplates.xml");
            var ser = new XmlSerializer(typeof(List<CustomBattleFormationTemplate>));
            ser.Serialize(File.OpenWrite(path), list);
        }
    }
}
