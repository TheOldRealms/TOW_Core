﻿using System;
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

        protected override bool CanCast(Agent casterAgent)
        {
            var hero = casterAgent.GetHero();
            if(hero != null && hero.GetExtendedInfo() != null)
            {
                var info = hero.GetExtendedInfo();
                if(info.CurrentWindsOfMagic < Template.WindsOfMagicCost)
                {
                    return false;
                }
            }
            return base.CanCast(casterAgent);
        }

        public override void Cast(Agent casterAgent)
        {
            base.Cast(casterAgent);
            var hero = casterAgent.GetHero();
            if (hero != null && hero.GetExtendedInfo() != null)
            {
                var info = hero.GetExtendedInfo();
                info.CurrentWindsOfMagic -= Template.WindsOfMagicCost;
            }
        }
    }
}