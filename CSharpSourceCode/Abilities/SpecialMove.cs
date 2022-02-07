using System;
using TaleWorlds.MountAndBlade;
using TOW_Core.Abilities.Scripts;

namespace TOW_Core.Abilities
{
    public class SpecialMove : Ability
    {
        public SpecialMove(AbilityTemplate template) : base(template)
        {

        }

        public override void ActivateAbility(Agent casterAgent)
        {
            base.ActivateAbility(casterAgent);
            _chargeLevel = 0;
        }

        public void AddCharge(float amount)
        {
            _chargeLevel += amount;
            _chargeLevel = Math.Min(100, _chargeLevel);
        }

        public bool IsUsing
        {
            get
            {
                return (ShadowStepScript)AbilityScript != null && !((ShadowStepScript)AbilityScript).IsFadinOut;
            }
        }

        public bool IsCharged => _chargeLevel >= 100f;
        private float _chargeLevel = 50f;
        public float ChargeLevel => _chargeLevel;
    }
}
