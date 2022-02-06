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

    public class Lore
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        private static Dictionary<string, Lore> _lores = new Dictionary<string, Lore>();
        private Lore(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public static List<Lore> GetAll()
        {
            if(_lores.Count == 0)
            {
                _lores.Add("testlore1", new Lore("testlore1", "Test Lore 1"));
            }
            return _lores.Values.ToList();
        }
        /*
        public static Lore GetLore(string id)
        {
            if (_lores.ContainsKey(id))
            {
                return _lores[id];
            }
            else
            {
                return null;
            }
        }*/
    }
}
