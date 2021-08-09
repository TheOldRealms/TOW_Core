using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TOW_Core.Abilities
{
    public class AbilityFactory
    {
        private static Dictionary<string, AbilityTemplate> _templates = new Dictionary<string, AbilityTemplate>();
        private static string _filename = "tow_abilitytemplates.xml";

        public static List<string> GetAllSpellNamesAsList()
        {
            List<string> list = new List<string>();
            var q = _templates.Distinct().Where(x => x.Value.AbilityType == AbilityType.Spell);
            foreach(var template in q)
            {
                list.Add(template.Value.StringID);
            }
            return list;
        }

        public static AbilityTemplate GetTemplate(string id)
        {
            return _templates.ContainsKey(id) ? _templates[id] : null;
        }

        public static void LoadTemplates()
        {
            var ser = new XmlSerializer(typeof(List<AbilityTemplate>));
            var path = Path.Combine(BasePath.Name, "Modules/TOW_Core/ModuleData/" + _filename);
            if (File.Exists(path))
            {
                var list = ser.Deserialize(File.OpenRead(path)) as List<AbilityTemplate>;
                foreach(var item in list)
                {
                    _templates.Add(item.StringID, item);
                }
            }
        }

        public static Ability CreateNew(string id)
        {
            Ability ability = null;
            if (_templates.ContainsKey(id))
            {
                ability = InitializeAbility(_templates[id]);
            }
            return ability;
        }

        private static Ability InitializeAbility(AbilityTemplate template)
        {
            Ability ability = null;
            if(template.AbilityType == AbilityType.Spell)
            {
                ability = new Spell(template);
                //and so on for the rest of the ability types
            }
            return ability;
        }
    }
}
