using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TOW_Core.Utilities.Extensions;

namespace TOW_Core.Abilities
{
    public class ItemBoundAbility : Ability
    {
        private int _chargeNum = 0;

        public ItemBoundAbility(AbilityTemplate template) : base(template) { }

        public void SetChargeNum(int amount)
        {
            _chargeNum = amount;
        }

        public override bool CanCast(Agent casterAgent)
        {
            return base.CanCast(casterAgent) && _chargeNum > 0 && Mission.Current.GetArtillerySlotsLeftForTeam(casterAgent.Team) > 0;
        }

        protected override void DoCast(Agent casterAgent)
        {
            base.DoCast(casterAgent);
            _chargeNum--;
        }
    }
}
