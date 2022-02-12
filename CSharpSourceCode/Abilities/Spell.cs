using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class Spell : Ability
    {
        public Spell(AbilityTemplate template) : base(template)
        {
        }

        public override bool CanCast(Agent casterAgent)
        {
            var hero = casterAgent.GetHero();
            if (hero != null && hero.GetExtendedInfo() != null)
            {
                var info = hero.GetExtendedInfo();
                if (info.CurrentWindsOfMagic < Template.WindsOfMagicCost)
                {
                    return false;
                }
            }
            return base.CanCast(casterAgent);
        }

        protected override void DoCast(Agent casterAgent)
        {
            base.DoCast(casterAgent);
            var hero = casterAgent.GetHero();
            if (hero != null && hero.GetExtendedInfo() != null)
            {
                var info = hero.GetExtendedInfo();
                info.CurrentWindsOfMagic -= Template.WindsOfMagicCost;
            }
        }
    }

    public class LoreObject
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string SpriteName { get; private set; }
        private static Dictionary<string, LoreObject> _lores = new Dictionary<string, LoreObject>();
        private LoreObject(string id, string name, string spritename)
        {
            ID = id;
            Name = name;
            SpriteName = spritename;
        }

        public static List<LoreObject> GetAll()
        {
            if(_lores.Count == 0)
            {
                _lores.Add("MinorMagic", new LoreObject("MinorMagic", "Minor Magic", "minormagic_symbol"));
                _lores.Add("LoreOfFire", new LoreObject("LoreOfFire", "Lore of Fire", "firemagic_symbol"));
                _lores.Add("LoreOfLight", new LoreObject("LoreOfLight", "Lore of Light", "lightmagic_symbol"));
                _lores.Add("LoreOfHeavens", new LoreObject("LoreOfHeavens", "Lore of Heavens", "celestial_symbol"));
                _lores.Add("DarkMagic", new LoreObject("DarkMagic", "Dark Magic", "darkmagic_symbol"));
                _lores.Add("Necromancy", new LoreObject("Necromancy", "Necromancy", "necromancy_symbol"));
            }
            return _lores.Values.ToList();
        }

        public static LoreObject GetLore(string id)
        {
            var lores = GetAll();
            return lores.First(lo => lo.ID == id);
        }
    }
}
