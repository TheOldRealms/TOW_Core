using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Crosshairs;

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
            foreach (var template in q)
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
                foreach (var item in list)
                {
                    _templates.Add(item.StringID, item);
                }
            }
        }

        public static Ability CreateNew(string id, Agent caster)
        {
            Ability ability = null;
            if (_templates.ContainsKey(id))
            {
                ability = InitializeAbility(_templates[id], caster);
            }
            return ability;
        }

        private static Ability InitializeAbility(AbilityTemplate template, Agent caster)
        {
            Ability ability = null;

            if (template.AbilityType == AbilityType.Spell)
            {
                ability = new Spell(template);
            }
            else if (template.AbilityType == AbilityType.Prayer)
            {
                ability = new Prayer(template);
            }
            else if (template.AbilityType == AbilityType.SpecialMove)
            {
                ability = new SpecialMove(template);
            }
            return ability;
        }

        public static AbilityCrosshair InitializeCrosshair(AbilityTemplate template, Agent caster)
        {
            AbilityCrosshair crosshair = null;
            switch (template.CrosshairType)
            {
                case CrosshairType.Projectile:
                    {
                        crosshair = new ProjectileCrosshair(template);
                        break;
                    }
                case CrosshairType.Targeted:
                    {
                        crosshair = new TargetedCrosshair(template, caster);
                        break;
                    }
                case CrosshairType.DirectionalAOE:
                    {
                        crosshair = new DirectionalAOECrosshair(template, caster);
                        break;
                    }
                case CrosshairType.CenteredAOE:
                    {
                        crosshair = new CenteredAOECrosshair(template, caster);
                        break;
                    }
                case CrosshairType.Pointer:
                    {
                        crosshair = new Pointer(template, caster);
                        break;
                    }
            }
            if (crosshair != null)
            {
                crosshair.Initialize();
            }
            return crosshair;
        }
    }
}
