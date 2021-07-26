using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TOW_Core.CampaignSupport.QuestBattleLocation
{
    public static class QuestBattleTemplateManager
    {
        private static List<QuestBattleTemplate> _templates = new List<QuestBattleTemplate>();
        private static Random _random = new Random();

        public static List<QuestBattleTemplate> AllTemplates
        {
            get
            {
                return _templates;
            }
        }

        public static QuestBattleTemplate GetRandomTemplate()
        {
            if (_templates.Count == 0) return null;
            int i = _random.Next(0, _templates.Count);
            return _templates[i];
        }

        public static void LoadQuestBattleTemplates()
        {
            try
            {
                var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_questbattle_templates.xml");
                var ser = new XmlSerializer(typeof(List<QuestBattleTemplate>));
                _templates = ser.Deserialize(File.OpenRead(path)) as List<QuestBattleTemplate>;
            }
            catch
            {
                TOW_Core.Utilities.TOWCommon.Log("Attempted to load questbattle templates but failed.", NLog.LogLevel.Error);
            }
        }

        internal static void WriteSampleXml()
        {
            var list = new List<QuestBattleTemplate>();
            var sampletemplate = new QuestBattleTemplate();
            sampletemplate.TemplateId = "chaosportal_01";
            sampletemplate.QuestBattleDescription = "You arrive at an active chaos portal. The vile thing pulses with arcane magic and has chaos forces are invading in minor numbers from their realm.";
            sampletemplate.QuestBattleSolveText = "Attempt to defeat the chaos forces present and close the portal.";
            sampletemplate.RewardItemId = "runefang_001";
            sampletemplate.SceneName = "";
            var trooptype1 = new QuestBattleTemplate.QuestBattleLocationNpcTuple();
            trooptype1.TroopId = "tow_skeleton_warrior";
            trooptype1.Count = 10;
            trooptype1.IsFriendly = false;
            var trooptype2 = new QuestBattleTemplate.QuestBattleLocationNpcTuple();
            trooptype2.TroopId = "tow_grave_guard";
            trooptype2.Count = 5;
            trooptype2.IsFriendly = false;
            var trooptype3 = new QuestBattleTemplate.QuestBattleLocationNpcTuple();
            trooptype3.TroopId = "tow_empire_recruit";
            trooptype3.Count = 10;
            trooptype3.IsFriendly = true;
            sampletemplate.TroopTypes.Add(trooptype1);
            sampletemplate.TroopTypes.Add(trooptype2);
            sampletemplate.TroopTypes.Add(trooptype3);
            list.Add(sampletemplate);
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/tow_questbattle_templates.xml");
            var ser = new XmlSerializer(typeof(List<QuestBattleTemplate>));
            ser.Serialize(File.OpenWrite(path), list);
        }
    }
}
