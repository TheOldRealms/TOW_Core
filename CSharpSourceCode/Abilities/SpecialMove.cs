using System;
using System.Collections;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TOW_Core.Abilities.Scripts;
using TOW_Core.Utilities;
using TOW_Core.Utilities.Extensions;
using static TaleWorlds.MountAndBlade.RidingOrder;

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

            if (casterAgent.HasMount)
                casterAgent.QuickDismount();
        }

        public override bool CanCast(Agent casterAgent)
        {
            return !IsCasting &&
                   !IsOnCooldown() &&
                   (casterAgent.IsPlayerControlled || (casterAgent.IsActive() && casterAgent.Health > 0 && casterAgent.GetMorale() > 1 && casterAgent.IsAbilityUser())
                   && !casterAgent.HasMount);
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
